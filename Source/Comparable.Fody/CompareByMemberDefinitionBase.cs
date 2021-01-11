using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Comparable.Fody
{
    public abstract class CompareByMemberDefinitionBase : ICompareByMemberDefinition
    {
        private readonly ICompareByMemberReference _memberReference;

        protected CompareByMemberDefinitionBase(ICompareByMemberReference memberReference, IComparableModuleDefine comparableModuleDefine)
        {
            _memberReference = memberReference;
            MemberTypeDefinition =
                comparableModuleDefine.FindComparableTypeDefinition(memberReference.MemberTypeReference);
            LocalVariable = MemberTypeDefinition.CreateVariableDefinition();
        }

        protected IComparableTypeDefinition MemberTypeDefinition { get; }

        protected MethodReference CompareTo => MemberTypeDefinition.GetCompareTo();

        public VariableDefinition LocalVariable { get; }

        public int Priority => _memberReference.Priority;

        public int DepthOfDependency => MemberTypeDefinition.DepthOfDependency;
        
        public abstract void AppendCompareTo(ILProcessor ilProcessor, ParameterDefinition parameterDefinition);
    }
}