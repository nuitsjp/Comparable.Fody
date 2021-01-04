using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.CompareStructWithConcreteType
{
    [Comparable]
    public struct StructProperty

    {
        [CompareBy]
        public CompareStructWithConcreteTypeValue Value { get; set; }

        public int NotCompareValue { get; set; }
    }
}
