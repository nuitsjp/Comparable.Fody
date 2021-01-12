using System.Collections.Generic;
using System.Linq;
using Fody;
using Mono.Cecil;

namespace Comparable.Fody
{
    public class ComparableTypeReference : IComparableTypeReference
    {
        private readonly List<ICompareByMemberReference> _members;

        public ComparableTypeReference(TypeReference selfReference, TypeDefinition selfDefinition, List<ICompareByMemberReference> members)
        {
            TypeReference = selfReference;
            TypeDefinition = selfDefinition;
            _members = members;
        }

        public TypeReference TypeReference { get; }
        public TypeDefinition TypeDefinition { get; }

        public int Depth =>
            _members.Empty() ? 0 : _members.Max(x => x.Depth) + 1;

        public IComparableTypeDefinition Resolve(IComparableModuleDefine comparableModuleDefine)
            => new ComparableTypeDefinition(this, _members.Select(x => x.Resolve(comparableModuleDefine)));
    }
}