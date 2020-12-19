using Comparable;
// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess
{
    [Comparable]
    public class StructWithIComparableDefined
    {
        [CompareBy]
        public int Value { get; set; }
    }
}