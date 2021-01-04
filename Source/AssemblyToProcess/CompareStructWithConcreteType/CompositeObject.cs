using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.CompareStructWithConcreteType
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
        public CompareStructWithConcreteTypeValue Value { get; set; }

        public int NotCompareValue { get; set; }
    }
}
