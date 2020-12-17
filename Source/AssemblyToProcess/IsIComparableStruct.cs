using Comparable;

namespace AssemblyToProcess
{
    [AddComparable]
    public class IsIComparableStruct
    {
        [CompareBy]
        public int Value { get; set; }
    }
}