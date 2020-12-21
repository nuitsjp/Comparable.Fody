using Comparable;
// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess
{
    [Comparable]
    public struct StructWithStructProperty

    {
        [CompareBy]
        public int Value { get; set; }

        public int NotCompareValue { get; set; }
    }
}
