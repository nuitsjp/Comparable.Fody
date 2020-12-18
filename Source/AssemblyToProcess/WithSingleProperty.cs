using System;
using Comparable;

namespace AssemblyToProcess
{
    [AddComparable]
    public class WithSingleProperty
    {
        [CompareBy(Priority = 1)]
        public int Value { get; set; }
        public int NotCompareValue { get; set; }
    }
}
