﻿using System;
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
                        MaxStackSize = 1,
                        InitLocals = true
                    }
                };
            compareToDefinition.Parameters.Add(new ParameterDefinition("obj", ParameterAttributes.None, module.TypeSystem.Object));

            compareToDefinition.Body.Variables.Add(new VariableDefinition(module.TypeSystem.Int32));
            var processor = compareToDefinition.Body.GetILProcessor();
            processor.Append(Instruction.Create(OpCodes.Nop));
            processor.Append(Instruction.Create(OpCodes.Ldc_I4_1));
            //processor.Append(Instruction.Create(OpCodes.Stloc_0));
            //processor.Append(Instruction.Create(OpCodes.Ldloc_0));
            processor.Append(Instruction.Create(OpCodes.Ret));

            type.Methods.Add(compareToDefinition);

            module.Write(@"AssemblyToProcess.dll");

            var assemblyPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "AssemblyToProcess.dll");
            Assembly a = Assembly.Load(File.ReadAllBytes(assemblyPath));
            // Get the type to use.
            Type myType = a.GetType("WithSingleProperty");
            var instance = a.CreateInstance("WithSingleProperty");
            var comparableInstance = instance as IComparable;
            var result = comparableInstance.CompareTo(comparableInstance);
        }
    }
}
