using Mono.Cecil;

namespace Comparable.Fody
{
    public class CompareByPropertyReference : CompareByMemberReference
    {
        public CompareByPropertyReference(PropertyDefinition self, TypeReference memberTypeReference, IComparableModuleDefine moduleDefine) :
            base(self, memberTypeReference, moduleDefine)
        {
            PropertyDefinition = self;
        }

        public PropertyDefinition PropertyDefinition { get; }

        public override ICompareByMemberDefinition Resolve(IComparableModuleDefine moduleDefine)
            => new CompareByPropertyDefinition(this, moduleDefine.Resolve(MemberTypeReference));
    }
}