namespace Comparable.Fody
{
    public interface ICompareByMemberReference
    {
        IComparableTypeReference MemberTypeReference { get; }
        int Priority { get; }
        int Depth { get; }

        ICompareByMemberDefinition Resolve(IComparableModuleDefine moduleDefine);
    }
}