using System;
using Comparable;

namespace AssemblyToProcess
{
    [AddComparable]
    public class WithSingleProperty
    {
        [CompareBy]
        public int Value { get; set; }
    }
}
