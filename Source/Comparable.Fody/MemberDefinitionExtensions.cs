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

        internal static int GetPriority(this IMemberDefinition propertyDefinition)
        {
            var compareBy = propertyDefinition.CustomAttributes
                .Single(x => x.AttributeType.Name == nameof(CompareByAttribute));
            if (!compareBy.HasProperties) return CompareByAttribute.DefaultPriority;

            return (int)compareBy.Properties
                .Single(x => x.Name == nameof(CompareByAttribute.Priority))
                .Argument.Value;
        }
    }
}