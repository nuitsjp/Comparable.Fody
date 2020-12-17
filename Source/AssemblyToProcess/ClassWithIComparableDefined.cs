using Comparable;

namespace AssemblyToProcess
{
    [AddComparable]
    public class ClassWithIComparableDefined
    {
        [CompareBy]
        public int Value { get; set; }
    }
}