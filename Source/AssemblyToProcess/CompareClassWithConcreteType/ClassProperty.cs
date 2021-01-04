using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.CompareClassWithConcreteType
{
    [Comparable]
    public class ClassProperty

    {
        [CompareBy]
        public CompareClassWithConcreteTypeValue Value { get; set; }

        public int NotCompareValue { get; set; }
    }
}
