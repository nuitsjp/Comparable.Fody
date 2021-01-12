using System.Linq;
using Mono.Cecil;

namespace Comparable.Fody
{
    public abstract class CompareByMemberReference : ICompareByMemberReference
    {
        protected CompareByMemberReference(IMemberDefinition self, IComparableTypeReference memberTypeReference)
        {
            MemberTypeReference = memberTypeReference;
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

        protected IComparableTypeReference MemberTypeReference { get; }
        public int Priority { get; }
        public int Depth => MemberTypeReference.Depth;

        public abstract ICompareByMemberDefinition Resolve(IComparableModuleDefine moduleDefine);
    }
}