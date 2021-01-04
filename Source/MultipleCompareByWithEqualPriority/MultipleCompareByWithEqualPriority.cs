using Comparable;
// ReSharper disable UnusedMember.Global

namespace MultipleCompareByWithEqualPriority
{
    [Comparable]
    public class MultipleCompareByWithEqualPriority
    {
        [CompareBy(Priority = 1)]
        public int Value0 { get; set; }
        [CompareBy(Priority = 1)]
        public int Value1 { get; set; }
    }
}
