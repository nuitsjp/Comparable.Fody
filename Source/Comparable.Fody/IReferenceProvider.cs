using Mono.Cecil;

namespace Comparable.Fody
{
    public interface IReferenceProvider
    {
        IComparableTypeReference Resolve(TypeReference typeReference);
        ICompareByMemberReference Resolve(FieldDefinition fieldDefinition);
        ICompareByMemberReference Resolve(PropertyDefinition propertyDefinition);
    }
}