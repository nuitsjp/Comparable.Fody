using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.CompareClassWithObject
{
    [Comparable]
    public class StructProperty

    {
        [CompareBy]
        public StructValue Value { get; set; }

        public int NotCompareValue { get; set; }
    }
}
