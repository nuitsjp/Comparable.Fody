using System.Collections.Generic;
using System.Linq;
using Fody;
using Mono.Cecil;

namespace Comparable.Fody
{
    public class ComparableModuleDefine : IComparableModuleDefine
    {
        private readonly Dictionary<TypeDefinition, IComparableTypeReference> _comparableTypeReferences = new();
        private readonly Dictionary<IComparableTypeReference, IComparableTypeDefinition> _comparableTypeDefinitions = new();
        private readonly Dictionary<IMemberDefinition, ICompareByMemberReference> _memberReferences = new();

        public IEnumerable<IComparableTypeDefinition> Resolve(IEnumerable<TypeDefinition> typeDefinitions)
        {
            foreach (var typeDefinition in typeDefinitions)
            {
                Resolve(typeDefinition);
            }

            foreach (var comparableTypeReference in _comparableTypeReferences.Values.OrderBy(x => x.Depth))
            {
                var definition = comparableTypeReference.Resolve(this);
                _comparableTypeDefinitions[comparableTypeReference] = definition;
            }

            return _comparableTypeDefinitions.Values;
        }

        public IComparableTypeReference Resolve(TypeReference typeReference)
        {
            if (typeReference.TryGetIComparableTypeDefinition(out var typeDefinition))
            {
                if (_comparableTypeReferences.TryGetValue(typeDefinition, out var comparableTypeReference))
                {
                    return comparableTypeReference;
                }

                List<ICompareByMemberReference> members;
                if (typeDefinition.HasCompareAttribute())
                {
                    var fields =
                        typeDefinition
                            .Fields
                            .Where(x => x.HasCompareByAttribute())
                            .Select(Resolve);

                    var properties =
                        typeDefinition
                            .Properties
                            .Where(x => x.HasCompareByAttribute())
                            .Select(Resolve);

                    members = fields.Union(properties).ToList();

                    if (members.Empty())
                    {
                        throw new WeavingException($"Specify CompareByAttribute for the any property of Type {typeDefinition.FullName}.");
                    }

                    if (1 < members
                        .GroupBy(x => x.Priority)
                        .Max(x => x.Count()))
                    {
                        throw new WeavingException($"Type {typeDefinition.FullName} defines multiple CompareBy with equal priority.");
                    }
                }
                else
                {
                    members = new();
                }

                var newTypeReference = new ComparableTypeReference(typeReference, typeDefinition, members);
                _comparableTypeReferences[typeDefinition] = newTypeReference;
                return newTypeReference;
            }

            throw new WeavingException(
                $"{typeReference.FullName} does not implement IComparable. Members that specifies CompareByAttribute should implement IComparable.");
        }

        private ICompareByMemberReference Resolve(FieldDefinition fieldDefinition)
        {
            if (_memberReferences.TryGetValue(fieldDefinition, out var typeReference))
            {
                return typeReference;
            }

            var newTypeReference = new CompareByFieldReference(fieldDefinition, Resolve(fieldDefinition.FieldType));
            _memberReferences[fieldDefinition] = newTypeReference;
            return newTypeReference;
        }

        private ICompareByMemberReference Resolve(PropertyDefinition propertyDefinition)
        {
            if (_memberReferences.TryGetValue(propertyDefinition, out var typeReference))
            {
                return typeReference;
            }

            var newTypeReference = new CompareByPropertyReference(propertyDefinition, Resolve(propertyDefinition.PropertyType));
            _memberReferences[propertyDefinition] = newTypeReference;
            return newTypeReference;
        }

        public IComparableTypeDefinition Resolve(IComparableTypeReference comparableTypeReference)
            => _comparableTypeDefinitions[comparableTypeReference];
    }
}