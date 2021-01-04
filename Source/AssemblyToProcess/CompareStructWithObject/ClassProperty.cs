using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.CompareStructWithObject
{
    [Comparable]
    public struct ClassProperty

    {
        [CompareBy]
        public CompareClassWithObjectValue Value { get; set; }

        public int NotCompareValue { get; set; }
    }
}
