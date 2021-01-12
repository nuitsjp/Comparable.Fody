using Mono.Cecil;

namespace Comparable.Fody
{
    public class CompareByPropertyReference : CompareByMemberReference
    {
        public CompareByPropertyReference(PropertyDefinition self, IComparableTypeReference memberTypeReference) :
            base(self, memberTypeReference)
        {
            PropertyDefinition = self;
        }

        public PropertyDefinition PropertyDefinition { get; }

        public override ICompareByMemberDefinition Resolve(IComparableModuleDefine moduleDefine)
            => new CompareByPropertyDefinition(this, moduleDefine.Resolve(MemberTypeReference));
    }
}