using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Comparable.Fody
{
    public interface IComparableTypeDefinition
    {
        bool IsStruct { get; }
        
        int Depth { get; }

        void ImplementCompareTo();

        MethodReference GetCompareTo();

        VariableDefinition CreateVariableDefinition();

        Instruction Box();

        Instruction Constrained();
    }
}