using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.CompareStructWithObject
{
    [Comparable]
    public struct StructProperty

    {
        [CompareBy]
        public StructValue Value { get; set; }

        public int NotCompareValue { get; set; }
    }
}
