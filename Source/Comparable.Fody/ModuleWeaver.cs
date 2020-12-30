using System;
using System.Collections.Generic;
using System.Linq;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using ParameterAttributes = Mono.Cecil.ParameterAttributes;

namespace Comparable.Fody
{
    public class ModuleWeaver : BaseModuleWeaver, IComparableModuleDefine
    {
        public override void Execute()
        {
            FindReferences();

            var typesOfComparable =
                ModuleDefinition
                    .Types
                    .Where(x => x.HasCompareAttribute())
                    .Select(x => new TypeOfComparable(x));

            var memberDefinition = ModuleDefinition
                .Types
                .SelectMany(x => x.Fields.Cast<IMemberDefinition>())
                .Union(ModuleDefinition.Types.SelectMany(x => x.Properties.Cast<IMemberDefinition>()))
                .Where(x => x.HasCompareByAttribute())
                .FirstOrDefault(x => !x.DeclaringType.HasCompareAttribute());
            if (memberDefinition != null)
            {
                throw new WeavingException($"Specify CompareAttribute for Type of {memberDefinition.DeclaringType.FullName}.");
            }

            foreach (var type in typesOfComparable)
            {
                ImplementIComparable(type);
            }
        }

        private void ImplementIComparable(TypeOfComparable typeOfComparable)
        {
            typeOfComparable.ImplementIComparable(ComparableInterface);
            var compareMembers = 
                GetCompareByProperties(typeOfComparable._typeDefinition)
                    .Union(GetCompareByFields(typeOfComparable._typeDefinition))
                    .ToArray();
            
            if (!typeOfComparable.Members.Any())
            {
                throw new WeavingException($"Specify CompareByAttribute for the any property of Type {typeOfComparable.FullName}.");
            }
            
            if (1 < typeOfComparable.Members
                .GroupBy(x => x.Priority)
                .Select(x => (Priority: x.Key, Count: x.Count()))
                .Max(x => x.Count))
            {
                throw new WeavingException($"Type {typeOfComparable.FullName} defines multiple CompareBy with equal priority.");
            }

            var compareToByConcreteType = AddCompareToByConcreteType(typeOfComparable, compareMembers);
            typeOfComparable.ImplementCompareToByObject(this, compareMembers, compareToByConcreteType);
            //AddCompareToByObject(typeOfComparable, compareMembers, compareToByConcreteType);
        }

        private MethodDefinition AddCompareToByConcreteType(TypeOfComparable weavingTarget,
            (Action<ILProcessor, ParameterDefinition> AppendCompareTo, VariableDefinition LocalVariable, int Priority)[] compareMembers)
        {
            var compareToDefinition =
                new MethodDefinition(
                    nameof(IComparable.CompareTo),
                    MethodAttributes.Public
                    | MethodAttributes.Final
                    | MethodAttributes.HideBySig
                    | MethodAttributes.NewSlot
                    | MethodAttributes.Virtual,
                    ModuleDefinition.TypeSystem.Int32)
                {
                    Body =
                    {
                        MaxStackSize = 2,
                        InitLocals = true
                    }
                };

            // Init arguments.
            var argumentObj = weavingTarget.CreateParameterDefinition("value");
            compareToDefinition.Parameters.Add(argumentObj);

            // Init local variables.
            var localResult = new VariableDefinition(ModuleDefinition.TypeSystem.Int32);
            compareToDefinition.Body.Variables.Add(localResult);
            foreach (var compareBy in compareMembers)
            {
                compareToDefinition.Body.Variables.Add(compareBy.LocalVariable);
            }

            // Labels for goto.
            var labelArgumentIsNotNull = Instruction.Create(OpCodes.Nop);
            var labelReturn = Instruction.Create(OpCodes.Nop);

            var processor = compareToDefinition.Body.GetILProcessor();

            if (weavingTarget.IsClass)
            {
                // if (obj == null)
                processor.Append(Instruction.Create(OpCodes.Ldarg_S, argumentObj));
                processor.Append(Instruction.Create(OpCodes.Ldnull));
                processor.Append(Instruction.Create(OpCodes.Ceq));
                processor.Append(Instruction.Create(OpCodes.Brfalse_S, labelArgumentIsNotNull));

                // return 1;
                processor.Append(Instruction.Create(OpCodes.Ldc_I4_1));
                processor.Append(Instruction.Create(OpCodes.Ret));
            }

            // ImplementType implementType = (ImplementType)obj;
            processor.Append(labelArgumentIsNotNull);
            //processor.Append(Instruction.Create(OpCodes.Ldarg_S, argumentObj));
            //processor.Append(weavingTarget.IsStruct()
            //    ? Instruction.Create(OpCodes.Unbox_Any, weavingTarget)
            //    : Instruction.Create(OpCodes.Castclass, weavingTarget));

            //processor.Append(Instruction.Create(OpCodes.Stloc_S, localCastedObject));


            // return Value.CompareTo(withSingleProperty.Value);
            foreach (var compareBy in compareMembers)
            {
                compareBy.AppendCompareTo(processor, argumentObj);
                processor.Append(Instruction.Create(OpCodes.Stloc_S, localResult));
                if (compareMembers.Last() != compareBy)
                {
                    processor.Append(Instruction.Create(OpCodes.Ldloc_S, localResult));
                    processor.Append(Instruction.Create(OpCodes.Ldc_I4_0));
                    processor.Append(Instruction.Create(OpCodes.Ceq));
                    processor.Append(Instruction.Create(OpCodes.Brfalse_S, labelReturn));
                }
            }

            processor.Append(labelReturn);
            processor.Append(Instruction.Create(OpCodes.Ldloc_S, localResult));
            processor.Append(Instruction.Create(OpCodes.Ret));

            weavingTarget.AddMethod(compareToDefinition);

            return compareToDefinition;
        }

        private IEnumerable<(Action<ILProcessor, ParameterDefinition> AppendCompareTo, VariableDefinition LocalVariable, int Priority)> GetCompareByProperties(TypeDefinition weavingTarget)
        {
            return weavingTarget.Properties
                .Where(x => x.HasCompareByAttribute())
                .Select(x =>
                {
                    var typeDefinition = ModuleDefinition.ImportReference(x.PropertyType).Resolve();
                    if (typeDefinition.IsNotImplementIComparable())
                    {
                        throw new WeavingException(
                            $"Property {x.Name} of Type {weavingTarget.FullName} does not implement IComparable; the property that specifies CompareByAttribute should implement IComparable.");
                    }
                    var compareTo = ModuleDefinition.ImportReference(
                        typeDefinition.Methods
                            .Single(methodDefinition =>
                                methodDefinition.Name == nameof(IComparable.CompareTo)
                                && methodDefinition.Parameters.Count == 1
                                && methodDefinition.Parameters.Single().ParameterType.FullName == typeDefinition.FullName));

                    var localVariable = new VariableDefinition(typeDefinition);


                    void AppendCompareTo(ILProcessor ilProcessor, ParameterDefinition parameterDefinition)
                    {
                        ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
                        ilProcessor.Append(Instruction.Create(OpCodes.Call, x.GetMethod));
                        if (typeDefinition.IsStruct())
                        {
                            ilProcessor.Append(Instruction.Create(OpCodes.Stloc_S, localVariable));
                            ilProcessor.Append(Instruction.Create(OpCodes.Ldloca_S, localVariable));
                        }


                        ilProcessor.Append(weavingTarget.IsStruct()
                            ? Instruction.Create(OpCodes.Ldarga_S, parameterDefinition)
                            : Instruction.Create(OpCodes.Ldarg_1));
                        
                        ilProcessor.Append(weavingTarget.IsStruct()
                            ? Instruction.Create(OpCodes.Call, x.GetMethod)
                            : Instruction.Create(OpCodes.Callvirt, x.GetMethod));
                        
                        ilProcessor.Append(typeDefinition.IsStruct()
                            ? Instruction.Create(OpCodes.Call, compareTo)
                            : Instruction.Create(OpCodes.Callvirt, compareTo));
                    }

                    return (
                        AppendCompareTo: (Action<ILProcessor, ParameterDefinition>) AppendCompareTo,
                        LocalVariable : localVariable,
                        Priority: x.GetPriority());
                });
        }

        private IEnumerable<(Action<ILProcessor, ParameterDefinition> AppendCompareTo, VariableDefinition LocalVariable, int Priority)> GetCompareByFields(TypeDefinition weavingTarget)
        {
            return weavingTarget.Fields
                .Where(x => x.HasCompareByAttribute())
                .Select(x =>
                {
                    var typeDefinition = ModuleDefinition.ImportReference(x.FieldType).Resolve();
                    if (typeDefinition.IsNotImplementIComparable())
                    {
                        throw new WeavingException(
                            $"Field {x.Name} of Type {weavingTarget.FullName} does not implement IComparable; the property that specifies CompareByAttribute should implement IComparable.");
                    }
                    var compareTo = ModuleDefinition.ImportReference(
                        typeDefinition.Methods
                            .Single(methodDefinition =>
                                methodDefinition.Name == nameof(IComparable.CompareTo)
                                && methodDefinition.Parameters.Count == 1
                                && methodDefinition.Parameters.Single().ParameterType.FullName == typeDefinition.FullName));

                    var localVariable = new VariableDefinition(typeDefinition);


                    void AppendCompareTo(ILProcessor ilProcessor, ParameterDefinition parameterDefinition)
                    {
                        ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
                        ilProcessor.Append(Instruction.Create(OpCodes.Ldfld, x));
                        if (typeDefinition.IsStruct())
                        {
                            ilProcessor.Append(Instruction.Create(OpCodes.Stloc_S, localVariable));
                            ilProcessor.Append(Instruction.Create(OpCodes.Ldloca_S, localVariable));
                        }
                        ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_1));
                        ilProcessor.Append(Instruction.Create(OpCodes.Ldfld, x));
                        ilProcessor.Append(typeDefinition.IsStruct()
                            ? Instruction.Create(OpCodes.Call, compareTo)
                            : Instruction.Create(OpCodes.Callvirt, compareTo));
                    }

                    return (
                        AppendCompareTo: (Action<ILProcessor, ParameterDefinition>)AppendCompareTo,
                        LocalVariable: localVariable,
                        Priority: x.GetPriority());
                });
        }

        private InterfaceImplementation ComparableInterface { get; set; }
        
        public TypeReference Int32 => ModuleDefinition.TypeSystem.Int32;

        public TypeReference Object => ModuleDefinition.TypeSystem.Object;

        public MethodReference ArgumentExceptionConstructor { get; private set; }
        
        private MethodReference CompareTo { get; set; }
        
        private void FindReferences()
        {
            var comparable = ModuleDefinition.ImportReference(FindTypeDefinition(nameof(IComparable)));
            ComparableInterface = new InterfaceImplementation(comparable);
            CompareTo = ModuleDefinition.ImportReference(comparable.Resolve().Methods.Single(x => x.Name == nameof(IComparable.CompareTo)));

            var argumentExceptionType = typeof(ArgumentException);
            var constructorInfo = argumentExceptionType.GetConstructors()
                .Single(x =>
                    x.GetParameters().Length == 1
                    && x.GetParameters().Single()?.ParameterType == typeof(string));
            ArgumentExceptionConstructor = ModuleDefinition.ImportReference(constructorInfo);
        }

        public override IEnumerable<string> GetAssembliesForScanning()
        {
            yield return "mscorlib";
            yield return "System";
            yield return "netstandard";
            yield return "System.Diagnostics.Tools";
            yield return "System.Diagnostics.Debug";
            yield return "System.Runtime";
            yield return "Comparable";
        }
    }

    public interface IComparableModuleDefine
    {
        TypeReference Int32 { get; }
        
        TypeReference Object { get; }

        MethodReference ArgumentExceptionConstructor { get; }
    }

    public class TypeOfComparable
    {
        public readonly TypeDefinition _typeDefinition;
        private readonly List<MemberOfCompareBy> _members;

        public TypeOfComparable(TypeDefinition typeDefinition)
        {
            _typeDefinition = typeDefinition;
            _members = _typeDefinition
                .Fields
                .Union(_typeDefinition.Properties.Cast<IMemberDefinition>())
                .Where(x => x.HasCompareByAttribute())
                .Select(x => new MemberOfCompareBy(x))
                .ToList();
        }

        public IEnumerable<MemberOfCompareBy> Members => _members;

        public bool HasNotCompareBy => !_members.Any();

        public string FullName => _typeDefinition.FullName;

        public bool IsClass => !_typeDefinition.IsStruct();

        public void ImplementIComparable(InterfaceImplementation comparable)
        {
            _typeDefinition.Interfaces.Add(comparable);
        }

        public ParameterDefinition CreateParameterDefinition(string name)
            => new (name, ParameterAttributes.None, _typeDefinition);

        public void AddMethod(MethodDefinition methodDefinition) => _typeDefinition.Methods.Add(methodDefinition);

        public void ImplementCompareToByObject(
            IComparableModuleDefine comparableModuleDefine,
            (Action<ILProcessor, ParameterDefinition> AppendCompareTo, VariableDefinition LocalVariable, int Priority)[] compareMembers,
            MethodDefinition compareToByConcreteType)
        {
            var compareToDefinition =
                new MethodDefinition(
                    nameof(IComparable.CompareTo),
                    MethodAttributes.Public
                    | MethodAttributes.Final
                    | MethodAttributes.HideBySig
                    | MethodAttributes.NewSlot
                    | MethodAttributes.Virtual,
                    comparableModuleDefine.Int32)
                {
                    Body =
                    {
                        MaxStackSize = 2,
                        InitLocals = true
                    }
                };

            // Init arguments.
            var argumentObj =
                new ParameterDefinition("obj", ParameterAttributes.None, comparableModuleDefine.Object);
            compareToDefinition.Parameters.Add(argumentObj);

            // Init local variables.
            var localResult = new VariableDefinition(comparableModuleDefine.Int32);
            compareToDefinition.Body.Variables.Add(localResult);
            foreach (var compareBy in compareMembers)
            {
                compareToDefinition.Body.Variables.Add(compareBy.LocalVariable);
            }

            // Labels for goto.
            var labelArgumentIsNotNull = Instruction.Create(OpCodes.Nop);
            var labelArgumentTypeMatched = Instruction.Create(OpCodes.Nop);
            var labelReturn = Instruction.Create(OpCodes.Nop);

            var processor = compareToDefinition.Body.GetILProcessor();

            // if (obj == null)
            processor.Append(Instruction.Create(OpCodes.Ldarg_S, argumentObj));
            processor.Append(Instruction.Create(OpCodes.Ldnull));
            processor.Append(Instruction.Create(OpCodes.Ceq));
            processor.Append(Instruction.Create(OpCodes.Brfalse_S, labelArgumentIsNotNull));

            // return 1;
            processor.Append(Instruction.Create(OpCodes.Ldc_I4_1));
            processor.Append(Instruction.Create(OpCodes.Ret));

            // if (!(obj is StructWithSingleField))
            processor.Append(labelArgumentIsNotNull);
            processor.Append(Instruction.Create(OpCodes.Ldarg_S, argumentObj));
            processor.Append(Instruction.Create(OpCodes.Isinst, _typeDefinition));
            processor.Append(Instruction.Create(OpCodes.Ldnull));
            processor.Append(Instruction.Create(OpCodes.Cgt_Un));
            processor.Append(Instruction.Create(OpCodes.Ldc_I4_0));
            processor.Append(Instruction.Create(OpCodes.Ceq));
            processor.Append(Instruction.Create(OpCodes.Brfalse_S, labelArgumentTypeMatched));

            // throw new ArgumentException("Object is not a WithSingleProperty");
            processor.Append(Instruction.Create(OpCodes.Ldstr, $"Object is not a {FullName}."));
            processor.Append(Instruction.Create(OpCodes.Newobj, comparableModuleDefine.ArgumentExceptionConstructor));
            processor.Append(Instruction.Create(OpCodes.Throw));

            processor.Append(labelArgumentTypeMatched);
            // ImplementType implementType = (ImplementType)obj;
            processor.Append(Instruction.Create(OpCodes.Ldarg_0));
            processor.Append(Instruction.Create(OpCodes.Ldarg_S, argumentObj));
            processor.Append(IsClass
                ? Instruction.Create(OpCodes.Castclass, _typeDefinition)
                : Instruction.Create(OpCodes.Unbox_Any, _typeDefinition));

            processor.Append(Instruction.Create(OpCodes.Call, compareToByConcreteType));
            processor.Append(Instruction.Create(OpCodes.Stloc_S, localResult));
            processor.Append(Instruction.Create(OpCodes.Ldloc_S, localResult));
            processor.Append(Instruction.Create(OpCodes.Ret));

            AddMethod(compareToDefinition);
        }
    }

    public class MemberOfCompareBy
    {
        private readonly IMemberDefinition _memberDefinition;

        public MemberOfCompareBy(IMemberDefinition memberDefinition)
        {
            _memberDefinition = memberDefinition;
            
        }

        public int Priority => _memberDefinition.GetPriority();
    }
}
