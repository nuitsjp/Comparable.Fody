using System;
using Comparable;

namespace AssemblyToProcess
{
    [AddComparable]
    public class WithDoubleProperty
    {
        [CompareBy]
        public int Value0 { get; set; }

        [CompareBy(Priority = 2)]
        public string Value1 { get; set; }

        public int NotCompareValue { get; set; }
    }
}
