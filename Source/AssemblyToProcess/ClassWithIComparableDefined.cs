using Comparable;

namespace AssemblyToProcess
{
    [Comparable.Comparable]
    public class ClassWithIComparableDefined
    {
        [CompareBy]
        public int Value { get; set; }
    }
}