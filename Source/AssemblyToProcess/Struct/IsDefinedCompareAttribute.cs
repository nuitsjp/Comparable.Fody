using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.Struct
{
    [Comparable]
    public struct IsDefinedCompareAttribute
    {
        [CompareBy]
        public int Value { get; set; }
    }
}