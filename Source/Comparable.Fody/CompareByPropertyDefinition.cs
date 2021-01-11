using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Comparable.Fody
{
    public class CompareByPropertyDefinition : CompareByMemberDefinitionBase
    {

        private readonly PropertyDefinition _thisProperty;

        public CompareByPropertyDefinition(IComparableModuleDefine comparableModuleDefine, CompareByPropertyReference propertyReference)
            : base(propertyReference, comparableModuleDefine)
        {
            _thisProperty = propertyReference.PropertyDefinition;
        }

        public override void AppendCompareTo(ILProcessor ilProcessor, ParameterDefinition parameterDefinition)
        {
            ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
            ilProcessor.Append(Instruction.Create(OpCodes.Call, _thisProperty.GetMethod.GetGenericMethodReference()));

            if (_thisProperty.PropertyType.IsGeneric()
                || MemberTypeDefinition.IsStruct)
            {
                ilProcessor.Append(Instruction.Create(OpCodes.Stloc_S, LocalVariable));
                ilProcessor.Append(Instruction.Create(OpCodes.Ldloca_S, LocalVariable));
            }

            if (_thisProperty.DeclaringType.IsStruct())
            {
                ilProcessor.Append(Instruction.Create(OpCodes.Ldarga_S, parameterDefinition));
                ilProcessor.Append(Instruction.Create(OpCodes.Call, _thisProperty.GetMethod.GetGenericMethodReference()));
            }
            else
            {
                ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_1));
                ilProcessor.Append(Instruction.Create(OpCodes.Callvirt, _thisProperty.GetMethod.GetGenericMethodReference()));
            }

            if (MemberTypeDefinition.IsStruct
                && CompareTo.ByObject())
            {
                ilProcessor.Append(MemberTypeDefinition.Box());
            }

            if (_thisProperty.PropertyType.IsGeneric())
            {
                ilProcessor.Append(MemberTypeDefinition.Box());
                ilProcessor.Append(MemberTypeDefinition.Constrained());
            }

            ilProcessor.Append(MemberTypeDefinition.IsStruct
                ? Instruction.Create(OpCodes.Call, CompareTo)
                : Instruction.Create(OpCodes.Callvirt, CompareTo));
        }
    }
}