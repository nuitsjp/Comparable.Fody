using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using ParameterAttributes = Mono.Cecil.ParameterAttributes;

namespace Comparable.WeavingConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = @"..\..\..\..\AssemblyToProcess\bin\Debug\netstandard2.0";
            var moduleFullName = Path.Combine(path, "AssemblyToProcess.dll");
            var module = ModuleDefinition.ReadModule(moduleFullName);
            var comparable = module.ImportReference(typeof(IComparable));
            var concreteComparableType = module.Types.Single(x => x.Name == "WithSingleProperty");
            var value = concreteComparableType.Properties.Single(x => x.Name == "Value");
            var getValue = value.GetMethod;

            var compareToMethod = typeof(Int32).GetMethods().Single(
                x => 
                    x.Name == "CompareTo" 
                    && x.GetParameters().SingleOrDefault()?.ParameterType == typeof(Int32));
            var compareTo = module.ImportReference(compareToMethod);


            concreteComparableType.Interfaces.Add(new InterfaceImplementation(comparable));

            var compareToDefinition =
                new MethodDefinition(
                    "CompareTo",
                    MethodAttributes.Public
                    | MethodAttributes.Final
                    | MethodAttributes.HideBySig
                    | MethodAttributes.NewSlot
                    | MethodAttributes.Virtual,
                    module.TypeSystem.Int32)
                {
                    Body =
                    {
                        MaxStackSize = 3,
                        InitLocals = true
                    }
                };
            compareToDefinition.Parameters.Add(new ParameterDefinition("obj", ParameterAttributes.None, module.TypeSystem.Object));

            var concreteComparableVariable = new VariableDefinition(concreteComparableType);
            compareToDefinition.Body.Variables.Add(concreteComparableVariable);
            compareToDefinition.Body.Variables.Add(new VariableDefinition(module.TypeSystem.Int32));
            compareToDefinition.Body.Variables.Add(new VariableDefinition(module.TypeSystem.Boolean));
            var argumentValueVariable = new VariableDefinition(module.TypeSystem.Int32);
            compareToDefinition.Body.Variables.Add(argumentValueVariable);
            var processor = compareToDefinition.Body.GetILProcessor();

            // obj is not null.
            var argumentIsNotNull = Instruction.Create(OpCodes.Nop);
            // throw ArgumentException
            var argumentIsWithSinglePropertyType = Instruction.Create(OpCodes.Nop);

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
            processor.Append(Instruction.Create(OpCodes.Isinst, concreteComparableType));
            processor.Append(Instruction.Create(OpCodes.Stloc_S, concreteComparableVariable));

            // if (withSingleProperty != null)
            processor.Append(Instruction.Create(OpCodes.Ldloc_S, concreteComparableVariable));
            processor.Append(Instruction.Create(OpCodes.Ldnull));
            processor.Append(Instruction.Create(OpCodes.Cgt_Un));
            processor.Append(Instruction.Create(OpCodes.Brtrue_S, argumentIsWithSinglePropertyType));

            // throw new ArgumentException("Object is not a WithSingleProperty");
            processor.Append(Instruction.Create(OpCodes.Ldstr, "Object is not a WithSingleProperty"));
            var argumentExceptionType = typeof(ArgumentException);
            var constructorInfo = argumentExceptionType.GetConstructors()
                .Single(x =>
                    x.GetParameters().Length == 1
                    && x.GetParameters().Single()?.ParameterType == typeof(string));
            var constructor = module.ImportReference(constructorInfo);
            processor.Append(Instruction.Create(OpCodes.Newobj, constructor));
            processor.Append(Instruction.Create(OpCodes.Throw));

            processor.Append(argumentIsWithSinglePropertyType);
            // return Value.CompareTo(withSingleProperty.Value);
            processor.Append(Instruction.Create(OpCodes.Ldarg_0));
            processor.Append(Instruction.Create(OpCodes.Call, getValue));
            processor.Append(Instruction.Create(OpCodes.Stloc_S, argumentValueVariable));
            processor.Append(Instruction.Create(OpCodes.Ldloca_S, argumentValueVariable));
            processor.Append(Instruction.Create(OpCodes.Ldloc_S, concreteComparableVariable));
            processor.Append(Instruction.Create(OpCodes.Callvirt, getValue));
            processor.Append(Instruction.Create(OpCodes.Call, compareTo));
            processor.Append(Instruction.Create(OpCodes.Ret));

            //processor.Append(il0040);
            //processor.Append(Instruction.Create(OpCodes.Ldloc_2));
            //processor.Append(Instruction.Create(OpCodes.Ret));

            concreteComparableType.Methods.Add(compareToDefinition);

            module.Write(@"AssemblyToProcess.dll");

            var assemblyPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "AssemblyToProcess.dll");
            Assembly a = Assembly.Load(File.ReadAllBytes(assemblyPath));
            // Get the type to use.
            Type myType = a.GetType("WithSingleProperty");
            var instance0 = (dynamic)a.CreateInstance("WithSingleProperty");
            var instance1 = (dynamic)a.CreateInstance("WithSingleProperty");
            instance1.Value = 1;
            var result = instance0.CompareTo(instance1);
            instance0.CompareTo(null);
        }
    }
}
