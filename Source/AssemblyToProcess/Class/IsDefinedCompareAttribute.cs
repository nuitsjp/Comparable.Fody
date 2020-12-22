using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.Class
{
    [Comparable]
    public class IsDefinedCompareAttribute
    {
        [CompareBy]
        public int Value { get; set; }
    }
}