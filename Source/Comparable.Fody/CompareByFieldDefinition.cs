using System;
using System.Linq;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Comparable.Fody
{
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
}