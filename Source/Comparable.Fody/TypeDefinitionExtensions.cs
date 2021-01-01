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

        internal static bool IsNotImplementIComparable(this TypeDefinition typeDefinition)
        {
            if (typeDefinition.Interfaces
                .Select(@interface => @interface.InterfaceType.FullName == nameof(IComparable)).Any())
            {
                return false;
            }

            if (typeDefinition.HasCompareAttribute())
            {
                return false;
            }

            return true;
        }

        internal static MethodReference GetCompareToMethodReference(this TypeDefinition typeDefinition)
            => typeDefinition.Methods
                .Single(methodDefinition =>
                    methodDefinition.Name == nameof(IComparable.CompareTo)
                    && methodDefinition.Parameters.Count == 1
                    && methodDefinition.Parameters.Single().ParameterType.FullName == typeDefinition.FullName);
    }
}