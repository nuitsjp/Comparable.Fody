using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
            return typeDefinition.BaseType is not null 
                   && typeDefinition.BaseType.Name == nameof(ValueType);
        }

        internal static bool TryGetIComparableTypeDefinition(this TypeReference typeReference, IComparableModuleDefine moduleDefine, out TypeDefinition comparableTypeDefinition)
        {
            if (typeReference.IsGenericParameter)
            {
                var genericParameter = (GenericParameter) typeReference;
                var comparableTypeDefinitions = genericParameter
                    .Constraints
                    .Select(x => x.ConstraintType.Resolve())
                    .Where(x => x.IsImplementIComparable())
                    .ToList();
                if (comparableTypeDefinitions.Empty())
                {
                    comparableTypeDefinition = null;
                    return false;
                }

                comparableTypeDefinition = comparableTypeDefinitions.First();
                return true;
            }
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
            if (typeDefinition.FullName == typeof(IComparable).FullName) return true;

            if (typeDefinition.Interfaces
                .Select(@interface => @interface.InterfaceType.FullName == typeof(IComparable).FullName).Any())
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