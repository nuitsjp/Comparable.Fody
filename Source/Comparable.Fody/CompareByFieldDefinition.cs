using System;
using System.Linq;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Comparable.Fody
{
    public class CompareByFieldDefinition : CompareByMemberDefinitionBase
    {

        private readonly FieldDefinition _thisField;

        public CompareByFieldDefinition(IComparableModuleDefine comparableModuleDefine, FieldDefinition fieldDefinition)
            : base(comparableModuleDefine, fieldDefinition, fieldDefinition.FieldType)
        {
            _thisField = fieldDefinition;
        }

        public override void AppendCompareTo(ILProcessor ilProcessor, ParameterDefinition parameterDefinition)
        {
            ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
            ilProcessor.Append(Instruction.Create(OpCodes.Ldfld, _thisField));
            if (MemberTypeDefinition.IsStruct)
            {
                ilProcessor.Append(Instruction.Create(OpCodes.Stloc_S, LocalVariable));
                ilProcessor.Append(Instruction.Create(OpCodes.Ldloca_S, LocalVariable));
            }
            ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_1));
            ilProcessor.Append(Instruction.Create(OpCodes.Ldfld, _thisField));
            ilProcessor.Append(MemberTypeDefinition.IsStruct
                ? Instruction.Create(OpCodes.Call, CompareTo)
                : Instruction.Create(OpCodes.Callvirt, CompareTo));
        }
    }
}