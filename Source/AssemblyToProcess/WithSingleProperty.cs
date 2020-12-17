using System;
using Comparable;

namespace AssemblyToProcess
{
    [AddComparable]
    public class WithSingleProperty
    {
        public int Value { get; set; }

        public int CompareTo2(object obj)
        {
            return 1;
        }
    }
}
