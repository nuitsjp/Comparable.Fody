using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.CompareClassWithConcreteType
{
    [Comparable]
    public class StructProperty

    {
        [CompareBy]
        public CompareStructWithConcreteTypeValue Value { get; set; }

        public CompareStructWithConcreteTypeValue NotCompareValue { get; set; }
    }
}
