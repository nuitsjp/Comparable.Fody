using System;
using Comparable;

namespace AssemblyToProcess
{
    [Comparable.Comparable]
    public class WithSingleProperty
    {
        [CompareBy(Priority = 1)]
        public int Value { get; set; }
        public int NotCompareValue { get; set; }
    }
}
