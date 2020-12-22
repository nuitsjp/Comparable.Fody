using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.Struct
{
    [Comparable]
    public struct ClassProperty

    {
        [CompareBy]
        public string Value { get; set; }

        public int NotCompareValue { get; set; }
    }
}
