using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.CompareStructWithObject
{
    [Comparable]
    public struct CompositeObject

    {
        [CompareBy]
        public InnerObject Value { get; set; }

        public int NotCompareValue { get; set; }
    }

    [Comparable]
    public struct InnerObject

    {
        [CompareBy]
        public CompareStructWithObjectValue Value { get; set; }

        public int NotCompareValue { get; set; }
    }
}
