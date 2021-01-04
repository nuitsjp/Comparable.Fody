using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.CompareStructWithConcreteType
{
    [Comparable]
    public struct ClassProperty

    {
        [CompareBy]
        public CompareClassWithConcreteTypeValue Value { get; set; }

        public int NotCompareValue { get; set; }
    }
}
