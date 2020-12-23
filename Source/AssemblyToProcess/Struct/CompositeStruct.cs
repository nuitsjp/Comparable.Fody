using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.Struct
{
    [Comparable]
    public struct CompositeStruct

    {
        [CompareBy]
        public InnerStruct Value { get; set; }

        public int NotCompareValue { get; set; }
    }

    [Comparable]
    public struct InnerStruct

    {
        [CompareBy]
        public int Value { get; set; }

        public int NotCompareValue { get; set; }
    }
}
