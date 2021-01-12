namespace Comparable.Fody
{
    public interface ICompareByMemberReference
    {
        int Priority { get; }
        int Depth { get; }

        ICompareByMemberDefinition Resolve(IComparableModuleDefine moduleDefine);
    }
}