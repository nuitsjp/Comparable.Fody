using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Comparable.Fody
{
    public interface IComparableTypeDefinition
    {
        string FullName { get; }

        bool IsStruct { get; }
        
        int DepthOfDependency { get; }

        MethodReference GetCompareTo();

        VariableDefinition CreateVariableDefinition();

        Instruction Box();

        Instruction Constrained();
    }
}