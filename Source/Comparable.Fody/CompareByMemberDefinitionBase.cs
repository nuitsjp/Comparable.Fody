using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Comparable.Fody
{
    public abstract class CompareByMemberDefinitionBase : ICompareByMemberDefinition
    {
        protected CompareByMemberDefinitionBase(IComparableTypeDefinition memberDefinition)
        {
            MemberTypeDefinition = memberDefinition;
            LocalVariable = MemberTypeDefinition.CreateVariableDefinition();
        }

        protected IComparableTypeDefinition MemberTypeDefinition { get; }

        protected MethodReference CompareTo => MemberTypeDefinition.GetCompareTo();

        public VariableDefinition LocalVariable { get; }

        public int DepthOfDependency => MemberTypeDefinition.DepthOfDependency;
        
        public abstract void AppendCompareTo(ILProcessor ilProcessor, ParameterDefinition parameterDefinition);
    }
}