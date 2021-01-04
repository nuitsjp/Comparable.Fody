using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.CompareClassWithObject
{
    [Comparable]
    public class CompositeObject

    {
        [CompareBy]
        public InnerObject Value { get; set; }

        public int NotCompareValue { get; set; }
    }

    [Comparable]
    public class InnerObject

    {
        [CompareBy]
        public CompareStructWithObjectValue Value { get; set; }

        public int NotCompareValue { get; set; }
    }
}
