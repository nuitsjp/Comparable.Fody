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
    public class ModuleWeaver : BaseModuleWeaver
    {
        public override void Execute()
        {
            FindReferences();

            var memberDefinition = ModuleDefinition
                .Types
                .SelectMany(x => x.Fields.Cast<IMemberDefinition>())
                .Union(ModuleDefinition.Types.SelectMany(x => x.Properties.Cast<IMemberDefinition>()))
                .Where(x => x.HasCompareByAttribute())
                .FirstOrDefault(x => !x.DeclaringType.HasCompareAttribute());
            if (memberDefinition != null)
            {
                throw new WeavingException("Specify CompareAttribute for Type of CompareByIsNotDefined.CompareByIsNotDefined.");
            }
            
            foreach (var type in ModuleDefinition.Types.Where(x => x.HasCompareAttribute()))
            {
                ImplementIComparable(type);
            }
        }

        private void ImplementIComparable(TypeDefinition weavingTarget)
        {
            weavingTarget.Interfaces.Add(ComparableInterface);
            var compareMembers = 
                GetCompareByProperties(weavingTarget)
                    .Union(GetCompareByFields(weavingTarget))
                    .ToArray();
            
            if (!compareMembers.Any())
            {
                throw new WeavingException($"Specify CompareByAttribute for the any property of Type {weavingTarget.FullName}.");
            }
            
            if (1 < compareMembers
                .GroupBy(x => x.Priority)
                .Select(x => (Priority: x.Key, Count: x.Count()))
                .Max(x => x.Count))
            {
                throw new WeavingException($"Type {weavingTarget.FullName} defines multiple CompareBy with equal priority.");
            }

            AddCompareToByConcreteType(weavingTarget, compareMembers);
            AddCompareToByObject(weavingTarget, compareMembers);
        }

        private void AddCompareToByObject(TypeDefinition weavingTarget,
            (Action<ILProcessor, VariableDefinition> AppendCompareTo, VariableDefinition LocalVariable, int Priority)[] compareMembers)
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
            var argumentObj =
                new ParameterDefinition("obj", ParameterAttributes.None, ModuleDefinition.TypeSystem.Object);
            compareToDefinition.Parameters.Add(argumentObj);

            // Init local variables.
            var localCastedObject = new VariableDefinition(weavingTarget);
            compareToDefinition.Body.Variables.Add(localCastedObject);
            var localResult = new VariableDefinition(ModuleDefinition.TypeSystem.Int32);
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
            processor.Append(Instruction.Create(OpCodes.Isinst, weavingTarget));
            processor.Append(Instruction.Create(OpCodes.Ldnull));
            processor.Append(Instruction.Create(OpCodes.Cgt_Un));
            processor.Append(Instruction.Create(OpCodes.Ldc_I4_0));
            processor.Append(Instruction.Create(OpCodes.Ceq));
            processor.Append(Instruction.Create(OpCodes.Brfalse_S, labelArgumentTypeMatched));

            // throw new ArgumentException("Object is not a WithSingleProperty");
            processor.Append(Instruction.Create(OpCodes.Ldstr, $"Object is not a {weavingTarget.FullName}."));
            processor.Append(Instruction.Create(OpCodes.Newobj, ArgumentExceptionConstructor));
            processor.Append(Instruction.Create(OpCodes.Throw));

            processor.Append(labelArgumentTypeMatched);
            // ImplementType implementType = (ImplementType)obj;
            processor.Append(Instruction.Create(OpCodes.Ldarg_S, argumentObj));
            if (weavingTarget.IsStruct())
            {
                processor.Append(Instruction.Create(OpCodes.Unbox_Any, weavingTarget));
            }
            else
            {
                processor.Append(Instruction.Create(OpCodes.Castclass, weavingTarget));
            }

            processor.Append(Instruction.Create(OpCodes.Stloc_S, localCastedObject));


            // return Value.CompareTo(withSingleProperty.Value);
            foreach (var compareBy in compareMembers)
            {
                compareBy.AppendCompareTo(processor, localCastedObject);
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

            weavingTarget.Methods.Add(compareToDefinition);
        }

        private void AddCompareToByConcreteType(TypeDefinition weavingTarget,
            (Action<ILProcessor, VariableDefinition> AppendCompareTo, VariableDefinition LocalVariable, int Priority)[] compareMembers)
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
            var argumentObj =
                new ParameterDefinition("obj", ParameterAttributes.None, weavingTarget);
            compareToDefinition.Parameters.Add(argumentObj);

            // Init local variables.
            var localCastedObject = new VariableDefinition(weavingTarget);
            compareToDefinition.Body.Variables.Add(localCastedObject);
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

            // if (obj == null)
            processor.Append(Instruction.Create(OpCodes.Ldarg_S, argumentObj));
            processor.Append(Instruction.Create(OpCodes.Ldnull));
            processor.Append(Instruction.Create(OpCodes.Ceq));
            processor.Append(Instruction.Create(OpCodes.Brfalse_S, labelArgumentIsNotNull));

            // return 1;
            processor.Append(Instruction.Create(OpCodes.Ldc_I4_1));
            processor.Append(Instruction.Create(OpCodes.Ret));

            // ImplementType implementType = (ImplementType)obj;
            processor.Append(Instruction.Create(OpCodes.Ldarg_S, argumentObj));
            if (weavingTarget.IsStruct())
            {
                processor.Append(Instruction.Create(OpCodes.Unbox_Any, weavingTarget));
            }
            else
            {
                processor.Append(Instruction.Create(OpCodes.Castclass, weavingTarget));
            }

            processor.Append(Instruction.Create(OpCodes.Stloc_S, localCastedObject));


            // return Value.CompareTo(withSingleProperty.Value);
            foreach (var compareBy in compareMembers)
            {
                compareBy.AppendCompareTo(processor, localCastedObject);
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

            weavingTarget.Methods.Add(compareToDefinition);
        }

        private IEnumerable<(Action<ILProcessor, VariableDefinition> AppendCompareTo, VariableDefinition LocalVariable, int Priority)> GetCompareByProperties(TypeDefinition weavingTarget)
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


                    void AppendCompareTo(ILProcessor ilProcessor, VariableDefinition castedObject)
                    {
                        ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
                        ilProcessor.Append(Instruction.Create(OpCodes.Call, x.GetMethod));
                        if (typeDefinition.IsStruct())
                        {
                            ilProcessor.Append(Instruction.Create(OpCodes.Stloc_S, localVariable));
                            ilProcessor.Append(Instruction.Create(OpCodes.Ldloca_S, localVariable));
                        }

                        ilProcessor.Append(weavingTarget.IsStruct()
                            ? Instruction.Create(OpCodes.Ldloca_S, castedObject)
                            : Instruction.Create(OpCodes.Ldloc_S, castedObject));
                        ilProcessor.Append(typeDefinition.IsStruct()
                            ? Instruction.Create(OpCodes.Call, x.GetMethod)
                            : Instruction.Create(OpCodes.Callvirt, x.GetMethod));
                        if (typeDefinition.IsStruct())
                        {
                            ilProcessor.Append(Instruction.Create(OpCodes.Call, compareTo));
                        }
                        else
                        {
                            ilProcessor.Append(Instruction.Create(OpCodes.Callvirt, compareTo));
                        }
                    }

                    return (
                        AppendCompareTo: (Action<ILProcessor, VariableDefinition>) AppendCompareTo,
                        LocalVariable : localVariable,
                        Priority: x.GetPriority());
                });
        }

        private IEnumerable<(Action<ILProcessor, VariableDefinition> AppendCompareTo, VariableDefinition LocalVariable, int Priority)> GetCompareByFields(TypeDefinition weavingTarget)
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


                    void AppendCompareTo(ILProcessor ilProcessor, VariableDefinition castedObject)
                    {
                        ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
                        ilProcessor.Append(Instruction.Create(OpCodes.Ldfld, x));
                        if (typeDefinition.IsStruct())
                        {
                            ilProcessor.Append(Instruction.Create(OpCodes.Stloc_S, localVariable));
                            ilProcessor.Append(Instruction.Create(OpCodes.Ldloca_S, localVariable));
                        }
                        ilProcessor.Append(Instruction.Create(OpCodes.Ldloc_S, castedObject));
                        ilProcessor.Append(Instruction.Create(OpCodes.Ldfld, x));
                        ilProcessor.Append(typeDefinition.IsStruct()
                            ? Instruction.Create(OpCodes.Call, compareTo)
                            : Instruction.Create(OpCodes.Callvirt, compareTo));
                    }

                    return (
                        AppendCompareTo: (Action<ILProcessor, VariableDefinition>)AppendCompareTo,
                        LocalVariable: localVariable,
                        Priority: x.GetPriority());
                });
        }

        private InterfaceImplementation ComparableInterface { get; set; }
        private MethodReference ArgumentExceptionConstructor { get; set; }
        
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
            yield return "ComparableAttribute";
        }
    }
}
