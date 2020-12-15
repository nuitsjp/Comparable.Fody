using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace Comparable.Fody
{
    public class ModuleWeaver : BaseModuleWeaver
    {
        private MethodInfo DebugWriteLine { get; } =
            typeof(System.Diagnostics.Debug)
                .GetTypeInfo()
                .DeclaredMethods
                .Where(x => x.Name == nameof(System.Diagnostics.Debug.WriteLine))
                .Single(x =>
                {
                    var parameters = x.GetParameters();
                    return parameters.Length == 1 &&
                           parameters[0].ParameterType == typeof(string);
                });

        public override void Execute()
        {
            FindReferences();
            foreach (var type in ModuleDefinition.Types.Where(x => !x.Name.Contains("Module")))
            {
                type.Interfaces.Add(IComparable);

                var compareToDefinition =
                    new MethodDefinition(
                        CompareTo.Name,
                        MethodAttributes.Public
                        | MethodAttributes.Final
                        | MethodAttributes.HideBySig
                        | MethodAttributes.NewSlot
                        | MethodAttributes.Virtual,
                        CompareTo.ReturnType)
                    {
                        Body =
                        {
                            MaxStackSize = CompareTo.Parameters.Count + 1,
                            InitLocals = true
                        }
                    };
                
                compareToDefinition.Parameters.Add(CompareTo.Parameters.First());

                var processor = compareToDefinition.Body.GetILProcessor();
                processor.Append(Instruction.Create(OpCodes.Nop));
                processor.Append(Instruction.Create(OpCodes.Ldc_I4_0));
                processor.Append(Instruction.Create(OpCodes.Stloc_0));
                var bar = Instruction.Create(OpCodes.Ldloc_0);
                processor.Append(Instruction.Create(OpCodes.Br_S, bar));
                processor.Append(bar);
                processor.Append(Instruction.Create(OpCodes.Ret));

                type.Methods.Add(compareToDefinition);
            }
            //var methods = ModuleDefinition
            //    .Types
            //    .SelectMany(x => x.Methods);
            //foreach (var method in methods)
            //{
            //    var processor = method.Body.GetILProcessor();
            //    var current = method.Body.Instructions.First();

            //    processor.InsertBefore(current, Instruction.Create(OpCodes.Nop));
            //    processor.InsertBefore(current, Instruction.Create(OpCodes.Ldstr, $"DEBUG: {method.DeclaringType.Name}#{method.Name}()"));
            //    processor.InsertBefore(current, Instruction.Create(OpCodes.Call, ModuleDefinition.ImportReference(DebugWriteLine)));
            //}
        }


        private InterfaceImplementation IComparable { get; set; }
        private MethodReference CompareTo { get; set; }
        
        private void FindReferences()
        {
            var comparableType = FindTypeDefinition("System.IComparable");
            IComparable = new InterfaceImplementation(ModuleDefinition.ImportReference(comparableType));
            CompareTo = ModuleDefinition.ImportReference(comparableType.Methods.Single(x => x.Name == "CompareTo"));
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
