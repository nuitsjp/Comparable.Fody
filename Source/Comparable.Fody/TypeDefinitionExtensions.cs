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

        internal static bool IsNotImplementIComparable(this TypeReference typeReference)
        {
            var typeDefinition = typeReference.Resolve();
            return !IsImplementIComparable(typeDefinition);
        }

        private static bool IsImplementIComparable(TypeDefinition typeDefinition)
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