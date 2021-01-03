using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.CompareClassWithObject
{
    [Comparable]
    public class ClassProperty

    {
        [CompareBy]
        public ClassValue Value { get; set; }

        public int NotCompareValue { get; set; }
    }
}
