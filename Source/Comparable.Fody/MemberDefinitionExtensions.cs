using System.Linq;
using Mono.Cecil;

namespace Comparable.Fody
{
    internal static class MemberDefinitionExtensions
    {
        internal static bool HasCompareByAttribute(this IMemberDefinition propertyDefinition)
        {
            return 0 != propertyDefinition.CustomAttributes.Count(x =>
                x.AttributeType.Name == nameof(CompareByAttribute));
        }
    }
}