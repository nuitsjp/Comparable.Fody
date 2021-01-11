using System.Collections.Generic;
using Mono.Cecil;

namespace Comparable.Fody
{
    public interface IReferenceProvider
    {
        IEnumerable<IComparableTypeReference> TypeReferences { get; }
        IComparableTypeReference Resolve(TypeReference typeReference);
        ICompareByMemberReference Resolve(FieldDefinition fieldDefinition);
        ICompareByMemberReference Resolve(PropertyDefinition propertyDefinition);
    }
}