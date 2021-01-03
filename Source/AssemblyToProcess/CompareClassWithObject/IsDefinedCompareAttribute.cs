using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.CompareClassWithObject
{
    [Comparable]
    public class IsDefinedCompareAttribute
    {
        [CompareBy]
        public StructValue Value { get; set; }
    }
}