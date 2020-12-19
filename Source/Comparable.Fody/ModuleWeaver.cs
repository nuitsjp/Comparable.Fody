using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
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
            foreach (var type in ModuleDefinition.Types.Where(IsDefinedForIComparable))
            {
                ImplementIComparable(type);
            }
        }

        private bool IsDefinedForIComparable(TypeDefinition typeDefinition)
        {
            return typeDefinition.CustomAttributes.Count(x => x.AttributeType.Name == nameof(ComparableAttribute)) == 1;
        }

        private void ImplementIComparable(TypeDefinition weavingTarget)
        {
            weavingTarget.Interfaces.Add(IComparableInterface);
            var compareProperties = 
                weavingTarget.Properties
                    .Where(x => x.HasCompareBy())
                    .Select(x =>
                    {
                        var propertyTypeReference = ModuleDefinition.ImportReference(x.PropertyType);
                        var propertyTypeDefinition = propertyTypeReference.Resolve();
                        if (!propertyTypeDefinition.Interfaces
                            .Select(x => x.InterfaceType.FullName == nameof(IComparable)).Any())
                        {
                            throw new WeavingException(
                                $"Property {x.Name} of Type {weavingTarget.FullName} does not implement IComparable; the property that specifies CompareByAttribute should implement IComparable.");
                        }
                        var compareTo = ModuleDefinition.ImportReference(
                            propertyTypeDefinition.Methods
                                .Single(x =>
                                    x.Name == nameof(IComparable.CompareTo)
                                    && x.Parameters.Count == 1
                                    && x.Parameters.Single().ParameterType.FullName == propertyTypeDefinition.FullName));
                        return (
                            GetValueDefinition : x.GetMethod,
                            CompareToReference : compareTo,
                            LocalVariable : new VariableDefinition(propertyTypeReference),
                            Priority: x.GetPriority());
                    })
                    .ToArray();

            if (!compareProperties.Any())
            {
                throw new WeavingException($"Specify CompareByAttribute for the any property of Type {weavingTarget.FullName}.");
            }

            if (1 < compareProperties
                .GroupBy(x => x.Priority)
                .Select(x => (Priority: x.Key, Count: x.Count()))
                .Max(x => x.Count))
            {
                throw new WeavingException($"Type {weavingTarget.FullName} defines multiple CompareBy with equal priority.");
            }

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
            foreach (var compareBy in compareProperties)
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

            // WithSingleProperty withSingleProperty = obj as WithSingleProperty;
            processor.Append(labelArgumentIsNotNull);
            processor.Append(Instruction.Create(OpCodes.Ldarg_S, argumentObj));
            processor.Append(Instruction.Create(OpCodes.Isinst, weavingTarget));
            processor.Append(Instruction.Create(OpCodes.Stloc_S, localCastedObject));

            // if (withSingleProperty != null)
            processor.Append(Instruction.Create(OpCodes.Ldloc_S, localCastedObject));
            processor.Append(Instruction.Create(OpCodes.Ldnull));
            processor.Append(Instruction.Create(OpCodes.Cgt_Un));
            processor.Append(Instruction.Create(OpCodes.Brtrue_S, labelArgumentTypeMatched));

            // throw new ArgumentException("Object is not a WithSingleProperty");
            processor.Append(Instruction.Create(OpCodes.Ldstr, $"Object is not a {weavingTarget.FullName}."));
            processor.Append(Instruction.Create(OpCodes.Newobj, ArgumentExceptionConstructor));
            processor.Append(Instruction.Create(OpCodes.Throw));

            processor.Append(labelArgumentTypeMatched);
            
            // return Value.CompareTo(withSingleProperty.Value);
            foreach (var compareBy in compareProperties)
            {
                processor.Append(Instruction.Create(OpCodes.Ldarg_0));
                processor.Append(Instruction.Create(OpCodes.Call, compareBy.GetValueDefinition));
                processor.Append(Instruction.Create(OpCodes.Stloc_S, compareBy.LocalVariable));
                processor.Append(Instruction.Create(OpCodes.Ldloca_S, compareBy.LocalVariable));
                processor.Append(Instruction.Create(OpCodes.Ldloc_S, localCastedObject));
                processor.Append(Instruction.Create(OpCodes.Callvirt, compareBy.GetValueDefinition));
                processor.Append(Instruction.Create(OpCodes.Call, compareBy.CompareToReference));
                processor.Append(Instruction.Create(OpCodes.Stloc_S, localResult));
                if (compareProperties.Last() != compareBy)
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

        private InterfaceImplementation IComparableInterface { get; set; }
        private MethodReference ArgumentExceptionConstructor { get; set; }
        
        private void FindReferences()
        {
            var comparableType = FindTypeDefinition(nameof(IComparable));
            IComparableInterface = new InterfaceImplementation(ModuleDefinition.ImportReference(comparableType));

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

    internal static class PropertyDefinitionExtensions
    {
        internal static bool HasCompareBy(this PropertyDefinition propertyDefinition)
        {
            return 0 != propertyDefinition.CustomAttributes.Count(x =>
                x.AttributeType.Name == nameof(CompareByAttribute));
        }

        internal static int GetPriority(this PropertyDefinition propertyDefinition)
        {
            var compareBy = propertyDefinition.CustomAttributes
                .Single(x => x.AttributeType.Name == nameof(CompareByAttribute));
            if (!compareBy.HasProperties) return CompareByAttribute.DefaultPriority;

            return (int)compareBy.Properties
                .Single(x => x.Name == nameof(CompareByAttribute.Priority))
                .Argument.Value;
        }
    }
}
