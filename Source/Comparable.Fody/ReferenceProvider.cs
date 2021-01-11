using System.Collections.Generic;
using Fody;
using Mono.Cecil;

namespace Comparable.Fody
{
    public class ReferenceProvider : IReferenceProvider
    {
        private readonly Dictionary<TypeDefinition, IComparableTypeReference> _typeReferences = new();
        private readonly Dictionary<IMemberDefinition, ICompareByMemberReference> _memberReferences = new();

        public IEnumerable<IComparableTypeReference> TypeReferences => _typeReferences.Values;

        public IComparableTypeReference Resolve(TypeReference typeReference)
        {
            if (typeReference.TryGetIComparableTypeDefinition(out var typeDefinition))
            {
                if (_typeReferences.TryGetValue(typeDefinition, out var comparableTypeReference))
                {
                    return comparableTypeReference;
                }

                var newTypeReference = new ComparableTypeReference(typeReference, typeDefinition, this);
                _typeReferences[typeDefinition] = newTypeReference;
                return newTypeReference;
            }

            throw new WeavingException(
                $"{typeReference.FullName} does not implement IComparable. Members that specifies CompareByAttribute should implement IComparable.");
        }

        public ICompareByMemberReference Resolve(FieldDefinition fieldDefinition)
        {
            if (_memberReferences.TryGetValue(fieldDefinition, out var typeReference))
            {
                return typeReference;
            }

            var newTypeReference = new CompareByFieldReference(fieldDefinition, fieldDefinition.FieldType, this);
            _memberReferences[fieldDefinition] = newTypeReference;
            return newTypeReference;
        }

        public ICompareByMemberReference Resolve(PropertyDefinition propertyDefinition)
        {
            if (_memberReferences.TryGetValue(propertyDefinition, out var typeReference))
            {
                return typeReference;
            }

            var newTypeReference = new CompareByPropertyReference(propertyDefinition, propertyDefinition.PropertyType, this);
            _memberReferences[propertyDefinition] = newTypeReference;
            return newTypeReference;
        }
    }
}