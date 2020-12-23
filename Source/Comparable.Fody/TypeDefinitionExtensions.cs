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
    }
}