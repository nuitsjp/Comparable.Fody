using Comparable;

namespace AssemblyToProcess
{
    [AddComparable]
    public class IsIComparableClass
    {
        [CompareBy]
        public int Value { get; set; }
    }
}