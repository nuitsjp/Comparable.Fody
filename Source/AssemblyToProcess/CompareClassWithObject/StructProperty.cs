using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.CompareClassWithObject
{
    [Comparable]
    public class StructProperty

    {
        [CompareBy]
        public CompareStructWithObjectValue Value { get; set; }

        public int NotCompareValue { get; set; }
    }
}
