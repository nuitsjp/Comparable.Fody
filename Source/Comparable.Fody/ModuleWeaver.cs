using System;
using System.Collections.Generic;
using System.Linq;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;

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
            return typeDefinition.CustomAttributes.Count(x => x.AttributeType.Name == "AddComparable") == 1;
        }

        private void ImplementIComparable(TypeDefinition target)
        {
            target.Interfaces.Add(IComparableInterface);

            var compareToDefinition =
                new MethodDefinition(
                    "CompareTo",
                    MethodAttributes.Public
                    | MethodAttributes.Final
                    | MethodAttributes.HideBySig
                    | MethodAttributes.NewSlot
                    | MethodAttributes.Virtual,
                    ModuleDefinition.TypeSystem.Int32)
                {
                    Body =
                    {
                        MaxStackSize = 3,
                        InitLocals = true
                    }
                };
            compareToDefinition.Parameters.Add(new ParameterDefinition("obj", ParameterAttributes.None, ModuleDefinition.TypeSystem.Object));

            var localVar0 = new VariableDefinition(target);
            var localVar2 = new VariableDefinition(ModuleDefinition.TypeSystem.Int32);
            var localVar4 = new VariableDefinition(ModuleDefinition.TypeSystem.Int32);
            compareToDefinition.Body.Variables.Add(localVar0);
            compareToDefinition.Body.Variables.Add(new VariableDefinition(ModuleDefinition.TypeSystem.Boolean));
            compareToDefinition.Body.Variables.Add(localVar2);
            compareToDefinition.Body.Variables.Add(localVar4);
            var processor = compareToDefinition.Body.GetILProcessor();

            // obj is not null.
            var argumentIsNotNull = Instruction.Create(OpCodes.Nop);
            // throw ArgumentException
            var argumentIsWithSinglePropertyType = Instruction.Create(OpCodes.Nop);
            // Return
            var ret = Instruction.Create(OpCodes.Nop);

            processor.Append(Instruction.Create(OpCodes.Nop));
            // if (obj == null)
            processor.Append(Instruction.Create(OpCodes.Ldarg_1));
            processor.Append(Instruction.Create(OpCodes.Ldnull));
            processor.Append(Instruction.Create(OpCodes.Ceq));
            processor.Append(Instruction.Create(OpCodes.Brfalse_S, argumentIsNotNull));

            // return 1;
            processor.Append(Instruction.Create(OpCodes.Ldc_I4_1));
            processor.Append(Instruction.Create(OpCodes.Ret));

            // WithSingleProperty withSingleProperty = obj as WithSingleProperty;
            processor.Append(argumentIsNotNull);
            processor.Append(Instruction.Create(OpCodes.Ldarg_1));
            processor.Append(Instruction.Create(OpCodes.Isinst, target));
            processor.Append(Instruction.Create(OpCodes.Stloc_S, localVar0));

            // if (withSingleProperty != null)
            processor.Append(Instruction.Create(OpCodes.Ldloc_S, localVar0));
            processor.Append(Instruction.Create(OpCodes.Ldnull));
            processor.Append(Instruction.Create(OpCodes.Cgt_Un));
            processor.Append(Instruction.Create(OpCodes.Brtrue_S, argumentIsWithSinglePropertyType));

            // throw new ArgumentException("Object is not a WithSingleProperty");
            processor.Append(Instruction.Create(OpCodes.Ldstr, $"Object is not a {target.FullName}."));
            processor.Append(Instruction.Create(OpCodes.Newobj, ArgumentExceptionConstructor));
            processor.Append(Instruction.Create(OpCodes.Throw));

            processor.Append(argumentIsWithSinglePropertyType);
            // return Value.CompareTo(withSingleProperty.Value);
            var value = target.Properties.Single(x => x.Name == "Value");
            var valueType = value.PropertyType;
            var getValue = value.GetMethod;

            var typedef = FindTypeDefinition(valueType.FullName);
            var compareToOfValue = FindTypeDefinition(valueType.FullName).Methods
                .Single(x =>
                    x.Name == "CompareTo"
                    && x.Parameters.Count == 1
                    && x.Parameters.Single().ParameterType.FullName == valueType.FullName);
            var compareTo = ModuleDefinition.ImportReference(compareToOfValue);

            processor.Append(Instruction.Create(OpCodes.Ldarg_0));
            processor.Append(Instruction.Create(OpCodes.Call, getValue));
            processor.Append(Instruction.Create(OpCodes.Stloc_S, localVar4));
            processor.Append(Instruction.Create(OpCodes.Ldloca_S, localVar4));
            processor.Append(Instruction.Create(OpCodes.Ldloc_S, localVar0));
            processor.Append(Instruction.Create(OpCodes.Callvirt, getValue));
            processor.Append(Instruction.Create(OpCodes.Call, compareTo));
            processor.Append(Instruction.Create(OpCodes.Stloc_S, localVar2));
            processor.Append(Instruction.Create(OpCodes.Br_S, ret));

            processor.Append(ret);
            processor.Append(Instruction.Create(OpCodes.Ldloc_2));
            processor.Append(Instruction.Create(OpCodes.Ret));

            target.Methods.Add(compareToDefinition);
        }


        private InterfaceImplementation IComparableInterface { get; set; }
        private MethodReference ArgumentExceptionConstructor { get; set; }
        
        private void FindReferences()
        {
            var comparableType = FindTypeDefinition("System.IComparable");
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
            yield return "Comparable";
        }
    }

}
