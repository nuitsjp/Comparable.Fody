using Comparable;

namespace AssemblyToProcess
{
    [Comparable.Comparable]
    public class StructWithIComparableDefined
    {
        [CompareBy]
        public int Value { get; set; }
    }
}