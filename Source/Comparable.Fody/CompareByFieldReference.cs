using Mono.Cecil;

namespace Comparable.Fody
{
    public class CompareByFieldReference : CompareByMemberReference
    {
        public CompareByFieldReference(FieldDefinition self, IComparableTypeReference memberTypeReference) : 
            base(self, memberTypeReference)
        {
            FieldDefinition = self;
        }

        public FieldDefinition FieldDefinition { get; }

        public override ICompareByMemberDefinition Resolve(IComparableModuleDefine moduleDefine)
            => new CompareByFieldDefinition( this, moduleDefine.Resolve(MemberTypeReference));
    }
}