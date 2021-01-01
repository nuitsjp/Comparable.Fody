using System.Linq;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MemberDefinitionExtensions = Comparable.Fody.MemberDefinitionExtensions;

namespace Comparable.Fody
{
    public class CompareByPropertyDefinition : CompareByMemberDefinitionBase
    {

        private readonly PropertyDefinition _thisProperty;

        public CompareByPropertyDefinition(IComparableModuleDefine comparableModuleDefine, PropertyDefinition propertyDefinition)
            : base(comparableModuleDefine, propertyDefinition, propertyDefinition.PropertyType)
        {
            _thisProperty = propertyDefinition;
        }

        public override void AppendCompareTo(ILProcessor ilProcessor, ParameterDefinition parameterDefinition)
        {
            ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
            ilProcessor.Append(Instruction.Create(OpCodes.Call, _thisProperty.GetMethod));
            if (MemberTypeDefinition.IsStruct)
            {
                ilProcessor.Append(Instruction.Create(OpCodes.Stloc_S, LocalVariable));
                ilProcessor.Append(Instruction.Create(OpCodes.Ldloca_S, LocalVariable));
            }

            if (_thisProperty.DeclaringType.IsStruct())
            {
                ilProcessor.Append(Instruction.Create(OpCodes.Ldarga_S, parameterDefinition));
                ilProcessor.Append(Instruction.Create(OpCodes.Call, _thisProperty.GetMethod));
            }
            else
            {
                ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_1));
                ilProcessor.Append(Instruction.Create(OpCodes.Callvirt, _thisProperty.GetMethod));
            }

            ilProcessor.Append(MemberTypeDefinition.IsStruct
                ? Instruction.Create(OpCodes.Call, CompareToMethodReference)
                : Instruction.Create(OpCodes.Callvirt, CompareToMethodReference));
        }
    }
}