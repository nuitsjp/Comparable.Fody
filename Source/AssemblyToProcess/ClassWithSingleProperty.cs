using Comparable;
// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess
{
    [Comparable]
    public class ClassWithSingleProperty
    {
        [CompareBy(Priority = 1)]
        public string Value { get; set; }
        public int NotCompareValue { get; set; }
    }
}
