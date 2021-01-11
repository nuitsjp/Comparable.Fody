using Mono.Cecil;

namespace Comparable.Fody
{
    public interface IComparableTypeReference
    {
        TypeReference TypeReference { get; }
        TypeDefinition TypeDefinition { get; }

        int Depth { get; }

        IComparableTypeDefinition Resolve(IComparableModuleDefine comparableModuleDefine);
    }
}