using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.CompareClassWithConcreteType
{
    [Comparable]
    public class IsDefinedCompareAttribute
    {
        [CompareBy]
        public CompareStructWithConcreteTypeValue Value { get; set; }
    }
}