using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.CompareClassWithConcreteType
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
        public CompareStructWithConcreteTypeValue Value { get; set; }

        public int NotCompareValue { get; set; }
    }
}
