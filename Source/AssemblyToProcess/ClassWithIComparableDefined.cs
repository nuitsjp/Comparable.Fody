using Comparable;

namespace AssemblyToProcess
{
    [Comparable]
    public class ClassWithIComparableDefined
    {
        [CompareBy]
        public int Value { get; set; }
    }
}