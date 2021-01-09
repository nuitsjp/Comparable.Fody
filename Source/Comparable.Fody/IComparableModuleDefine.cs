using Mono.Cecil;

namespace Comparable.Fody
{
    public interface IComparableModuleDefine
    {
        TypeReference Int32 { get; }
        
        TypeReference Object { get; }

        TypeReference GenericIComparable { get; }

        // ReSharper disable once InconsistentNaming
        InterfaceImplementation IComparable { get; }
        MethodReference ArgumentExceptionConstructor { get; }

        IComparableTypeDefinition FindComparableTypeDefinition(IMemberDefinition memberDefinition, TypeReference memberTypeReference);

        TypeReference ImportReference(TypeReference typeReference);

        MethodReference ImportReference(MethodReference methodReference);
    }
}