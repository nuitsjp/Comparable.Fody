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

            FieldTypeDefinition = comparableModuleDefine.FindComparableTypeDefinition(FieldDefinition.FieldType);
            if (FieldTypeDefinition.IsNotImplementIComparable)
            {
                throw new WeavingException(
                    $"Field {fieldDefinition.Name} of Type {fieldDefinition.DeclaringType.FullName} does not implement IComparable; the property that specifies CompareByAttribute should implement IComparable.");
            }
            CompareToMethodReference = comparableModuleDefine.ImportReference(
                FieldTypeDefinition.GetCompareToMethodReference());

            LocalVariable = FieldTypeDefinition.CreateVariableDefinition();
        }

        private FieldDefinition FieldDefinition { get; }

        private IComparableTypeDefinition FieldTypeDefinition { get; }

        private MethodReference CompareToMethodReference { get; }
        
        public VariableDefinition LocalVariable { get; }

        public int Priority => FieldDefinition.GetPriority();
        public void AppendCompareTo(ILProcessor ilProcessor, ParameterDefinition parameterDefinition)
        {
            ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
            ilProcessor.Append(Instruction.Create(OpCodes.Ldfld, FieldDefinition));
            if (FieldTypeDefinition.IsStruct)
            {
                ilProcessor.Append(Instruction.Create(OpCodes.Stloc_S, LocalVariable));
                ilProcessor.Append(Instruction.Create(OpCodes.Ldloca_S, LocalVariable));
            }
            ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_1));
            ilProcessor.Append(Instruction.Create(OpCodes.Ldfld, FieldDefinition));
            ilProcessor.Append(FieldTypeDefinition.IsStruct
                ? Instruction.Create(OpCodes.Call, CompareToMethodReference)
                : Instruction.Create(OpCodes.Callvirt, CompareToMethodReference));
        }
    }
}