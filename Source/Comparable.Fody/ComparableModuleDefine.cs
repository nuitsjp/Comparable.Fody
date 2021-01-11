using System;
using System.Collections.Generic;
using System.Linq;
using Fody;
using Mono.Cecil;

namespace Comparable.Fody
{
    public class ComparableModuleDefine : IComparableModuleDefine
    {
        private readonly IModuleWeaver _moduleWeaver;
        private readonly ModuleDefinition _moduleDefinition;
        private readonly Dictionary<TypeDefinition, IComparableTypeReference> _typeReferences = new();
        private readonly Dictionary<IMemberDefinition, ICompareByMemberReference> _memberReferences = new();
        private readonly Dictionary<IComparableTypeReference, IComparableTypeDefinition> _comparableTypeDefinitions = new();

        public ComparableModuleDefine(IModuleWeaver moduleWeaver, ModuleDefinition moduleDefinition)
        {
            _moduleWeaver = moduleWeaver;
            _moduleDefinition = moduleDefinition;
        }

        public TypeReference Int32 => _moduleDefinition.TypeSystem.Int32;
        public TypeReference Object => _moduleDefinition.TypeSystem.Object;
        public TypeReference IComparable { get; private set; }
        public TypeReference GenericIComparable { get; private set; }
        public MethodReference ArgumentExceptionConstructor { get; private set; }

        public IEnumerable<IComparableTypeDefinition> Resolve(IEnumerable<TypeDefinition> typeDefinitions)
        {
            FindReferences();

            foreach (var typeDefinition in typeDefinitions)
            {
                Resolve(typeDefinition);
            }

            foreach (var comparableTypeReference in _typeReferences.Values.OrderBy(x => x.Depth))
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

        public IComparableTypeDefinition Resolve(IComparableTypeReference comparableTypeReference)
            => _comparableTypeDefinitions[comparableTypeReference];

        public MethodReference ImportReference(MethodReference methodReference)
            => _moduleDefinition.ImportReference(methodReference);

        private void FindReferences()
        {
            IComparable = _moduleDefinition.ImportReference(_moduleWeaver.FindTypeDefinition(typeof(IComparable).FullName));
            GenericIComparable = _moduleDefinition.ImportReference(_moduleWeaver.FindTypeDefinition(typeof(IComparable<>).FullName!));

            var argumentExceptionType = typeof(ArgumentException);
            var constructorInfo = argumentExceptionType.GetConstructors()
                .Single(x =>
                    x.GetParameters().Length == 1
                    && x.GetParameters().Single()?.ParameterType == typeof(string));
            ArgumentExceptionConstructor = _moduleDefinition.ImportReference(constructorInfo);

        }

    }
}