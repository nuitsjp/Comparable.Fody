using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.Class.ComparedInConcreteClass
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
        public int Value { get; set; }

        public int NotCompareValue { get; set; }
    }
}
