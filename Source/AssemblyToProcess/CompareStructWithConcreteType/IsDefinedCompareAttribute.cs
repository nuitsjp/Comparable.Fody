using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.CompareStructWithConcreteType
{
    [Comparable]
    public struct IsDefinedCompareAttribute
    {
        [CompareBy]
        public int Value { get; set; }
    }
}