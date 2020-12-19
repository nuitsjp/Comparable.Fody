using Comparable;

namespace AssemblyToProcess
{
    [Comparable]
    public class StructWithIComparableDefined
    {
        [CompareBy]
        public int Value { get; set; }
    }
}