using System.Linq;
using Mono.Cecil;

namespace Comparable.Fody
{
    public abstract class CompareByMemberReference : ICompareByMemberReference
    {
        protected CompareByMemberReference(IMemberDefinition self, TypeReference memberTypeReference, IReferenceProvider referenceProvider)
        {
            MemberTypeReference = referenceProvider.Resolve(memberTypeReference);
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

        public IComparableTypeReference MemberTypeReference { get; }
        public int Priority { get; }
        public int Depth => MemberTypeReference.Depth;

        public abstract ICompareByMemberDefinition Resolve(IComparableModuleDefine moduleDefine);
    }

    public class CompareByFieldReference : CompareByMemberReference
    {
        public CompareByFieldReference(FieldDefinition self, TypeReference memberTypeReference, IReferenceProvider referenceProvider) : 
            base(self, memberTypeReference, referenceProvider)
        {
            FieldDefinition = self;
        }

        public FieldDefinition FieldDefinition { get; }

        public override ICompareByMemberDefinition Resolve(IComparableModuleDefine moduleDefine)
            => new CompareByFieldDefinition(moduleDefine, this);
    }

    public class CompareByPropertyReference : CompareByMemberReference
    {
        public CompareByPropertyReference(PropertyDefinition self, TypeReference memberTypeReference, IReferenceProvider referenceProvider) :
            base(self, memberTypeReference, referenceProvider)
        {
            PropertyDefinition = self;
        }

        public PropertyDefinition PropertyDefinition { get; }

        public override ICompareByMemberDefinition Resolve(IComparableModuleDefine moduleDefine)
            => new CompareByPropertyDefinition(moduleDefine, this);
    }
}