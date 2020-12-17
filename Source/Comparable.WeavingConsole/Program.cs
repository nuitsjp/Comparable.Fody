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
            var type = module.Types.Single(x => x.Name == "WithSingleProperty");
            var value = type.Properties.Single(x => x.Name == "Value");
 
            var compareTo2 = type.Methods.Single(x => x.Name == "CompareTo2");

            type.Interfaces.Add(new InterfaceImplementation(comparable));

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

            compareToDefinition.Body.Variables.Add(new VariableDefinition(type));
            compareToDefinition.Body.Variables.Add(new VariableDefinition(module.TypeSystem.Boolean));
            compareToDefinition.Body.Variables.Add(new VariableDefinition(module.TypeSystem.Int32));
            compareToDefinition.Body.Variables.Add(new VariableDefinition(module.TypeSystem.Boolean));
            compareToDefinition.Body.Variables.Add(new VariableDefinition(module.TypeSystem.Int32));
            var processor = compareToDefinition.Body.GetILProcessor();

            // obj is not null.
            var il000d = Instruction.Create(OpCodes.Nop);
            // Last return
            var il0040 = Instruction.Create(OpCodes.Nop);
            // throw ArgumentException
            var il0035 = Instruction.Create(OpCodes.Nop);

            processor.Append(Instruction.Create(OpCodes.Nop));
            // if (obj == null)
            processor.Append(Instruction.Create(OpCodes.Ldarg_1));
            processor.Append(Instruction.Create(OpCodes.Ldnull));
            processor.Append(Instruction.Create(OpCodes.Ceq));
            processor.Append(Instruction.Create(OpCodes.Stloc_1));
            processor.Append(Instruction.Create(OpCodes.Ldloc_1));
            processor.Append(Instruction.Create(OpCodes.Brfalse_S, il000d));

            // return 1;
            processor.Append(Instruction.Create(OpCodes.Ldc_I4_1));
            processor.Append(Instruction.Create(OpCodes.Ret));

            // obj is not null.
            // WithSingleProperty withSingleProperty = obj as WithSingleProperty;
            processor.Append(Instruction.Create(OpCodes.Br_S, il0040));
            processor.Append(Instruction.Create(OpCodes.Ldarg_1));
            processor.Append(Instruction.Create(OpCodes.Isinst, type));
            processor.Append(Instruction.Create(OpCodes.Stloc_0));
            // if (withSingleProperty != null)
            processor.Append(Instruction.Create(OpCodes.Ldloc_0));
            processor.Append(Instruction.Create(OpCodes.Ldnull));
            processor.Append(Instruction.Create(OpCodes.Cgt_Un));
            processor.Append(Instruction.Create(OpCodes.Stloc_3));
            processor.Append(Instruction.Create(OpCodes.Ldloc_3));
            processor.Append(Instruction.Create(OpCodes.Brfalse_S, il0035));

            // return Value.CompareTo(withSingleProperty.Value);
            processor.Append(Instruction.Create(OpCodes.Ldarg_0));
            processor.Append(Instruction.Create(OpCodes.Call, getValue));
            processor.Append(Instruction.Create(OpCodes.Stloc_S, 4));
            processor.Append(Instruction.Create(OpCodes.Ldloca_S, 4));
            processor.Append(Instruction.Create(OpCodes.Ldloc_0));
            processor.Append(Instruction.Create(OpCodes.Callvirt, getValue));


            type.Methods.Add(compareToDefinition);

            module.Write(@"AssemblyToProcess.dll");

            var assemblyPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "AssemblyToProcess.dll");
            Assembly a = Assembly.Load(File.ReadAllBytes(assemblyPath));
            // Get the type to use.
            Type myType = a.GetType("WithSingleProperty");
            var instance = a.CreateInstance("WithSingleProperty");
            var comparableInstance = (IComparable)instance;
            var result = comparableInstance.CompareTo(comparableInstance);
        }
    }
}
