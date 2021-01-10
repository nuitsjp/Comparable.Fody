using System.Linq;
using Mono.Cecil;

namespace Comparable.Fody
{
    public class CompareByMemberReference : ICompareByMemberReference
    {
        private readonly IComparableTypeReference _memberTypeReference;

        public CompareByMemberReference(IMemberDefinition self, TypeReference memberTypeReference, IReferenceProvider referenceProvider)
        {
            _memberTypeReference = referenceProvider.Resolve(memberTypeReference);
            var compareBy = self.CustomAttributes
                .Single(x => x.AttributeType.Name == nameof(CompareByAttribute));
            if (compareBy.HasProperties)
            {
                Priority = (int)compareBy.Properties
                    .Single(x => x.Name == nameof(CompareByAttribute.Priority))
                    .Argument.Value;
            }
            else
            {
                // If the property has a default value, "HasProperties" will be false.
                Priority = CompareByAttribute.DefaultPriority;
            }

        }
        public int Priority { get; }
        public int Depth => _memberTypeReference.Depth;
    }
}