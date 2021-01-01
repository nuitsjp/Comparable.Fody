using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.Class.ComparedInConcreteClass
{
    [Comparable]
    public class StructProperty

    {
        [CompareBy]
        public int Value { get; set; }

        public int NotCompareValue { get; set; }
    }
}
