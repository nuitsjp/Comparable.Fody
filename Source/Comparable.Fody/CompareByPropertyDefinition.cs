using System.Linq;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MemberDefinitionExtensions = Comparable.Fody.MemberDefinitionExtensions;

namespace Comparable.Fody
{
    public class CompareByPropertyDefinition : CompareByMemberDefinitionBase
    {

        private readonly PropertyDefinition _memberDefinition;

        public CompareByPropertyDefinition(IComparableModuleDefine comparableModuleDefine, PropertyDefinition memberDefinition)
            : base(comparableModuleDefine, memberDefinition, memberDefinition.PropertyType)
        {
            _memberDefinition = memberDefinition;
        }

        public override void AppendCompareTo(ILProcessor ilProcessor, ParameterDefinition parameterDefinition)
        {
            ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
            ilProcessor.Append(Instruction.Create(OpCodes.Call, _memberDefinition.GetMethod));
            if (MemberTypeDefinition.IsStruct)
            {
                ilProcessor.Append(Instruction.Create(OpCodes.Stloc_S, LocalVariable));
                ilProcessor.Append(Instruction.Create(OpCodes.Ldloca_S, LocalVariable));
            }

            if (_memberDefinition.DeclaringType.IsStruct())
            {
                ilProcessor.Append(Instruction.Create(OpCodes.Ldarga_S, parameterDefinition));
                ilProcessor.Append(Instruction.Create(OpCodes.Call, _memberDefinition.GetMethod));
            }
            else
            {
                ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_1));
                ilProcessor.Append(Instruction.Create(OpCodes.Callvirt, _memberDefinition.GetMethod));
            }

            ilProcessor.Append(MemberTypeDefinition.IsStruct
                ? Instruction.Create(OpCodes.Call, CompareToMethodReference)
                : Instruction.Create(OpCodes.Callvirt, CompareToMethodReference));
        }
    }
}