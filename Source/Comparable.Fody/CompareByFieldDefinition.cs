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
            ilProcessor.Append(Instruction.Create(OpCodes.Ldfld, GetGenericFieldReference()));
            if (MemberTypeDefinition.IsStruct)
            {
                ilProcessor.Append(Instruction.Create(OpCodes.Stloc_S, LocalVariable));
                ilProcessor.Append(Instruction.Create(OpCodes.Ldloca_S, LocalVariable));
            }
            ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_1));
            ilProcessor.Append(Instruction.Create(OpCodes.Ldfld, GetGenericFieldReference()));

            if (MemberTypeDefinition.IsStruct
                && CompareTo.ByObject())
            {
                ilProcessor.Append(MemberTypeDefinition.Box());
            }

            ilProcessor.Append(MemberTypeDefinition.IsStruct
                ? Instruction.Create(OpCodes.Call, CompareTo)
                : Instruction.Create(OpCodes.Callvirt, CompareTo));
        }

        private FieldReference GetGenericFieldReference()
        {
            if (!_thisField.ContainsGenericParameter) return _thisField;

            var declaringType = new GenericInstanceType(_thisField.DeclaringType);
            foreach (var parameter in _thisField.DeclaringType.GenericParameters)
            {
                declaringType.GenericArguments.Add(parameter);
            }
            return new FieldReference(_thisField.Name, _thisField.FieldType, declaringType);
        }
    }
}