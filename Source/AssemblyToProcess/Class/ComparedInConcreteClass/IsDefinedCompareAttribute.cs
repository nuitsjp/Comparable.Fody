using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.Class.ComparedInConcreteClass
{
    [Comparable]
    public class IsDefinedCompareAttribute
    {
        [CompareBy]
        public int Value { get; set; }
    }
}