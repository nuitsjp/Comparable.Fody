using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Comparable.Fody
{
    public interface IComparableTypeDefinition
    {
        string FullName { get; }

        bool IsClass { get; }
        
        bool IsStruct { get; }
        
        int DepthOfDependency { get; }

        bool IsNotImplementIComparable { get; }

        MethodReference GetCompareToMethodReference();

        VariableDefinition CreateVariableDefinition();
    }
}