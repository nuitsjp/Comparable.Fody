using System;
using Comparable;

namespace MultipleCompareByWithEqualPriority
{
    [Comparable]
    public class MultipleCompareByWithEqualPriority
    {
        [CompareBy(1)]
        public int Value0 { get; set; }
        [CompareBy(1)]
        public int Value1 { get; set; }
    }
}
