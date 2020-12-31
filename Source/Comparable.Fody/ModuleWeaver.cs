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
                    .Select(x => new ComparableTypeDefinition(this, x));

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

        private void ImplementIComparable(ComparableTypeDefinition comparableTypeDefinition)
        {
            comparableTypeDefinition.ImplementIComparable(ComparableInterface);
            if (!comparableTypeDefinition.Members.Any())
            {
                throw new WeavingException($"Specify CompareByAttribute for the any property of Type {comparableTypeDefinition.FullName}.");
            }
            
            if (1 < comparableTypeDefinition.Members
                .GroupBy(x => x.Priority)
                .Select(x => (Priority: x.Key, Count: x.Count()))
                .Max(x => x.Count))
            {
                throw new WeavingException($"Type {comparableTypeDefinition.FullName} defines multiple CompareBy with equal priority.");
            }

            comparableTypeDefinition.ImplementCompareTo();
        }

        private InterfaceImplementation ComparableInterface { get; set; }
        
        public TypeReference Int32 => ModuleDefinition.TypeSystem.Int32;

        public TypeReference Object => ModuleDefinition.TypeSystem.Object;

        public MethodReference ArgumentExceptionConstructor { get; private set; }
        public TypeReference ImportReference(TypeReference typeReference)
        {
            return ModuleDefinition.ImportReference(typeReference);
        }

        public MethodReference ImportReference(MethodReference methodReference)
        {
            return ModuleDefinition.ImportReference(methodReference);
        }

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

        TypeReference ImportReference(TypeReference typeReference);

        MethodReference ImportReference(MethodReference methodReference);
    }

    public class ComparableTypeDefinition
    {
        private readonly List<ICompareByMemberDefinition> _members;

        public ComparableTypeDefinition(IComparableModuleDefine comparableModuleDefine, TypeDefinition typeDefinition)
        {
            ComparableModuleDefine = comparableModuleDefine;
            TypeDefinition = typeDefinition;
            _members = TypeDefinition
                .Fields.Where(x => x.HasCompareByAttribute()).Select(x => new CompareByFieldDefinition(ComparableModuleDefine, x)).Cast<ICompareByMemberDefinition>()
                .Union(TypeDefinition.Properties.Where(x => x.HasCompareByAttribute()).Select(x => new CompareByPropertyDefinition(ComparableModuleDefine, x)))
                .ToList();
        }
        public IComparableModuleDefine ComparableModuleDefine { get; }
        public TypeDefinition TypeDefinition { get; }

        public IEnumerable<ICompareByMemberDefinition> Members => _members;

        public bool HasNotCompareBy => !_members.Any();

        public string FullName => TypeDefinition.FullName;

        public bool IsClass => !TypeDefinition.IsStruct();
        
        private MethodDefinition CompareToByObject { get; set; }

        public void ImplementIComparable(InterfaceImplementation comparable)
        {
            TypeDefinition.Interfaces.Add(comparable);
        }

        public void AddMethod(MethodDefinition methodDefinition) => TypeDefinition.Methods.Add(methodDefinition);

        public void ImplementCompareTo()
        {
            ImplementCompareToByConcreteType();
            ImplementCompareToByObject();
        }


        private void ImplementCompareToByConcreteType()
        {
            CompareToByObject =
                new MethodDefinition(
                    nameof(IComparable.CompareTo),
                    MethodAttributes.Public
                    | MethodAttributes.Final
                    | MethodAttributes.HideBySig
                    | MethodAttributes.NewSlot
                    | MethodAttributes.Virtual,
                    ComparableModuleDefine.Int32)
                {
                    Body =
                    {
                        MaxStackSize = 2,
                        InitLocals = true
                    }
                };

            // Init arguments.
            var argumentObj = new ParameterDefinition("value", ParameterAttributes.None, TypeDefinition);
            CompareToByObject.Parameters.Add(argumentObj);

            // Init local variables.
            var localResult = new VariableDefinition(ComparableModuleDefine.Int32);
            CompareToByObject.Body.Variables.Add(localResult);
            foreach (var member in _members)
            {
                CompareToByObject.Body.Variables.Add(member.LocalVariable);
            }

            // Labels for goto.
            var labelArgumentIsNotNull = Instruction.Create(OpCodes.Nop);
            var labelReturn = Instruction.Create(OpCodes.Nop);

            var processor = CompareToByObject.Body.GetILProcessor();

            if (IsClass)
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

            // return Value.CompareTo(withSingleProperty.Value);
            foreach (var member in _members)
            {
                member.AppendCompareTo(processor, argumentObj);
                processor.Append(Instruction.Create(OpCodes.Stloc_S, localResult));
                if (_members.Last() != member)
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

            AddMethod(CompareToByObject);
        }


        private void ImplementCompareToByObject()
        {
            var compareToDefinition =
                new MethodDefinition(
                    nameof(IComparable.CompareTo),
                    MethodAttributes.Public
                    | MethodAttributes.Final
                    | MethodAttributes.HideBySig
                    | MethodAttributes.NewSlot
                    | MethodAttributes.Virtual,
                    ComparableModuleDefine.Int32)
                {
                    Body =
                    {
                        MaxStackSize = 2,
                        InitLocals = true
                    }
                };

            // Init arguments.
            var argumentObj =
                new ParameterDefinition("obj", ParameterAttributes.None, ComparableModuleDefine.Object);
            compareToDefinition.Parameters.Add(argumentObj);

            // Init local variables.
            var localResult = new VariableDefinition(ComparableModuleDefine.Int32);
            compareToDefinition.Body.Variables.Add(localResult);
            foreach (var member in _members)
            {
                CompareToByObject.Body.Variables.Add(member.LocalVariable);
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
            processor.Append(Instruction.Create(OpCodes.Isinst, TypeDefinition));
            processor.Append(Instruction.Create(OpCodes.Ldnull));
            processor.Append(Instruction.Create(OpCodes.Cgt_Un));
            processor.Append(Instruction.Create(OpCodes.Ldc_I4_0));
            processor.Append(Instruction.Create(OpCodes.Ceq));
            processor.Append(Instruction.Create(OpCodes.Brfalse_S, labelArgumentTypeMatched));

            // throw new ArgumentException("Object is not a WithSingleProperty");
            processor.Append(Instruction.Create(OpCodes.Ldstr, $"Object is not a {FullName}."));
            processor.Append(Instruction.Create(OpCodes.Newobj, ComparableModuleDefine.ArgumentExceptionConstructor));
            processor.Append(Instruction.Create(OpCodes.Throw));

            processor.Append(labelArgumentTypeMatched);
            // ImplementType implementType = (ImplementType)obj;
            processor.Append(Instruction.Create(OpCodes.Ldarg_0));
            processor.Append(Instruction.Create(OpCodes.Ldarg_S, argumentObj));
            processor.Append(IsClass
                ? Instruction.Create(OpCodes.Castclass, TypeDefinition)
                : Instruction.Create(OpCodes.Unbox_Any, TypeDefinition));

            processor.Append(Instruction.Create(OpCodes.Call, CompareToByObject));
            processor.Append(Instruction.Create(OpCodes.Stloc_S, localResult));
            processor.Append(Instruction.Create(OpCodes.Ldloc_S, localResult));
            processor.Append(Instruction.Create(OpCodes.Ret));

            AddMethod(compareToDefinition);
        }
    }

    public interface ICompareByMemberDefinition
    {
        VariableDefinition LocalVariable { get; }
        int Priority { get; }

        void AppendCompareTo(ILProcessor ilProcessor, ParameterDefinition parameterDefinition);
    }
    public class CompareByFieldDefinition : ICompareByMemberDefinition
    {
        public CompareByFieldDefinition(IComparableModuleDefine comparableModuleDefine, FieldDefinition fieldDefinition)
        {
            FieldDefinition = fieldDefinition;

            FieldTypeDefine = comparableModuleDefine.ImportReference(FieldDefinition.FieldType).Resolve();
            if (FieldTypeDefine.IsNotImplementIComparable())
            {
                throw new WeavingException(
                    $"Field {fieldDefinition.Name} of Type {fieldDefinition.DeclaringType.FullName} does not implement IComparable; the property that specifies CompareByAttribute should implement IComparable.");
            }
            CompareTo = comparableModuleDefine.ImportReference(
                FieldTypeDefine.Methods
                    .Single(methodDefinition =>
                        methodDefinition.Name == nameof(IComparable.CompareTo)
                        && methodDefinition.Parameters.Count == 1
                        && methodDefinition.Parameters.Single().ParameterType.FullName == FieldTypeDefine.FullName));

            LocalVariable = new VariableDefinition(FieldTypeDefine);
        }

        private FieldDefinition FieldDefinition { get; }

        private TypeDefinition FieldTypeDefine { get; }

        private MethodReference CompareTo { get; }
        
        public VariableDefinition LocalVariable { get; }

        public int Priority => FieldDefinition.GetPriority();
        public void AppendCompareTo(ILProcessor ilProcessor, ParameterDefinition parameterDefinition)
        {
            ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
            ilProcessor.Append(Instruction.Create(OpCodes.Ldfld, FieldDefinition));
            if (FieldTypeDefine.IsStruct())
            {
                ilProcessor.Append(Instruction.Create(OpCodes.Stloc_S, LocalVariable));
                ilProcessor.Append(Instruction.Create(OpCodes.Ldloca_S, LocalVariable));
            }
            ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_1));
            ilProcessor.Append(Instruction.Create(OpCodes.Ldfld, FieldDefinition));
            ilProcessor.Append(FieldTypeDefine.IsStruct()
                ? Instruction.Create(OpCodes.Call, CompareTo)
                : Instruction.Create(OpCodes.Callvirt, CompareTo));
        }
    }

    public class CompareByPropertyDefinition : ICompareByMemberDefinition
    {
        public CompareByPropertyDefinition(IComparableModuleDefine comparableModuleDefine, PropertyDefinition propertyDefinition)
        {
            PropertyDefinition = propertyDefinition;
            TypeDefinition = comparableModuleDefine.ImportReference(propertyDefinition.PropertyType).Resolve();
            if (TypeDefinition.IsNotImplementIComparable())
            {
                throw new WeavingException(
                    $"Property {propertyDefinition.Name} of Type {propertyDefinition.DeclaringType.FullName} does not implement IComparable; the property that specifies CompareByAttribute should implement IComparable.");
            }
            CompareTo = comparableModuleDefine.ImportReference(
                TypeDefinition.Methods
                    .Single(methodDefinition =>
                        methodDefinition.Name == nameof(IComparable.CompareTo)
                        && methodDefinition.Parameters.Count == 1
                        && methodDefinition.Parameters.Single().ParameterType.FullName == TypeDefinition.FullName));

            LocalVariable = new VariableDefinition(TypeDefinition);
        }

        private PropertyDefinition PropertyDefinition { get; set; }

        private TypeDefinition TypeDefinition { get; }

        private MethodReference CompareTo { get; }

        public VariableDefinition LocalVariable { get; }

        public int Priority => PropertyDefinition.GetPriority();
        
        public void AppendCompareTo(ILProcessor ilProcessor, ParameterDefinition parameterDefinition)
        {
            ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
            ilProcessor.Append(Instruction.Create(OpCodes.Call, PropertyDefinition.GetMethod));
            if (TypeDefinition.IsStruct())
            {
                ilProcessor.Append(Instruction.Create(OpCodes.Stloc_S, LocalVariable));
                ilProcessor.Append(Instruction.Create(OpCodes.Ldloca_S, LocalVariable));
            }


            ilProcessor.Append(PropertyDefinition.DeclaringType.IsStruct()
                ? Instruction.Create(OpCodes.Ldarga_S, parameterDefinition)
                : Instruction.Create(OpCodes.Ldarg_1));

            ilProcessor.Append(PropertyDefinition.DeclaringType.IsStruct()
                ? Instruction.Create(OpCodes.Call, PropertyDefinition.GetMethod)
                : Instruction.Create(OpCodes.Callvirt, PropertyDefinition.GetMethod));

            ilProcessor.Append(TypeDefinition.IsStruct()
                ? Instruction.Create(OpCodes.Call, CompareTo)
                : Instruction.Create(OpCodes.Callvirt, CompareTo));
        }
    }

}
