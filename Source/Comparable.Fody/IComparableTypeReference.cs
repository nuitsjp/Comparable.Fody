namespace Comparable.Fody
{
    public interface IComparableTypeReference
    {
        string FullName { get; }

        int Depth { get; }

        IComparableTypeDefinition Resolve(IComparableModuleDefine comparableModuleDefine);
    }
}