using System;
using System.Linq;
using Mono.Cecil;

namespace Comparable.Fody
{
    internal static class TypeDefinitionExtensions
    {
        internal static bool HasCompareAttribute(this TypeDefinition typeDefinition)
        {
            return 0 != typeDefinition.CustomAttributes.Count(x => 
                x.AttributeType.Name == nameof(ComparableAttribute));
        }

        internal static bool IsStruct(this TypeDefinition typeDefinition)
        {
            return typeDefinition.BaseType.Name == nameof(ValueType);
        }

        internal static bool TryGetIComparableTypeDefinition(this TypeReference typeReference, out TypeDefinition comparableTypeDefinition)
        {
            var typeDefinition = typeReference.Resolve();
            if (typeDefinition.IsImplementIComparable())
            {
                comparableTypeDefinition = typeDefinition;
                return true;
            }

            comparableTypeDefinition = null;
            return false;
        }

        private static bool IsImplementIComparable(this TypeDefinition typeDefinition)
        {
            if (typeDefinition.Interfaces
                .Select(@interface => @interface.InterfaceType.FullName == nameof(IComparable)).Any())
            {
                return true;
            }

            if (typeDefinition.HasCompareAttribute())
            {
                return true;
            }

            return false;
        }

        internal static MethodReference GetCompareToMethodReference(this TypeDefinition typeDefinition)
        {
            var compareTo = typeDefinition.Methods
                .SingleOrDefault(methodDefinition =>
                    methodDefinition.Name == nameof(IComparable.CompareTo)
                    && methodDefinition.Parameters.Count == 1
                    && methodDefinition.Parameters.Single().ParameterType.FullName == typeDefinition.FullName);
            if (compareTo is not null) return compareTo;

            return typeDefinition.Methods
                .Single(methodDefinition =>
                    methodDefinition.Name == nameof(IComparable.CompareTo)
                    && methodDefinition.Parameters.Count == 1
                    && methodDefinition.Parameters.Single().ParameterType.FullName == typeof(Object).FullName);
        }
    }
}