using System.Collections.Generic;
using Mono.Cecil;

namespace Comparable.Fody
{
    public interface IComparableModuleDefine
    {
        TypeReference Int32 { get; }
        
        TypeReference Object { get; }

        // ReSharper disable once InconsistentNaming
        TypeReference IComparable { get; }

        TypeReference GenericIComparable { get; }

        MethodReference ArgumentExceptionConstructor { get; }

        MethodReference ImportReference(MethodReference methodReference);

        // ReSharper disable once UnusedMemberInSuper.Global
        IEnumerable<IComparableTypeDefinition> Resolve(IEnumerable<TypeDefinition> typeDefinitions);
        IComparableTypeReference Resolve(TypeReference typeReference);
        ICompareByMemberReference Resolve(FieldDefinition fieldDefinition);
        ICompareByMemberReference Resolve(PropertyDefinition propertyDefinition);
        IComparableTypeDefinition Resolve(IComparableTypeReference comparableTypeReference);
    }
}