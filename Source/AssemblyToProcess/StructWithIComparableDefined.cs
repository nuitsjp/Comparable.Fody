using Comparable;

namespace AssemblyToProcess
{
    [AddComparable]
    public class StructWithIComparableDefined
    {
        [CompareBy]
        public int Value { get; set; }
    }
}