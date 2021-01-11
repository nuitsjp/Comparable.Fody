using System.Collections.Generic;
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

        IComparableTypeDefinition FindComparableTypeDefinition(IComparableTypeReference comparableTypeReference);

        MethodReference ImportReference(MethodReference methodReference);

        IEnumerable<IComparableTypeDefinition> Resolve(IEnumerable<TypeDefinition> typeDefinitions);

        IComparableTypeReference Resolve(TypeReference typeReference);
        ICompareByMemberReference Resolve(FieldDefinition fieldDefinition);
        ICompareByMemberReference Resolve(PropertyDefinition propertyDefinition);

    }
}