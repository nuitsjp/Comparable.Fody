using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.CompareClassWithObject
{
    [Comparable]
    public class ClassProperty

    {
        [CompareBy]
        public CompareClassWithObjectValue Value { get; set; }

        public int NotCompareValue { get; set; }
    }
}
