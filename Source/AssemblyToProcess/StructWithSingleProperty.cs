using Comparable;
// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess
{
    [Comparable]
    public struct StructWithSingleProperty

    {
        [CompareBy]
        public string Value { get; set; }

        public int NotCompareValue { get; set; }
    }
}
