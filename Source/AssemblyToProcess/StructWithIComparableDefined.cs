using Comparable;
// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess
{
    [Comparable]
    public struct StructWithIComparableDefined
    {
        [CompareBy]
        public int Value { get; set; }
    }
}