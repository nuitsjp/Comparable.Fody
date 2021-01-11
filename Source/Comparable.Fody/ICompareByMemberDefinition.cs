using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Comparable.Fody
{
    public interface ICompareByMemberDefinition
    {
        VariableDefinition LocalVariable { get; }
        int DepthOfDependency { get; }

        void AppendCompareTo(ILProcessor ilProcessor, ParameterDefinition parameterDefinition);
    }
}