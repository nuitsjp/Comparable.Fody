using System;
using Comparable;

namespace AssemblyToProcess
{
    [Comparable]
    public class WithSingleField
    
    {
        [CompareBy]
        private int _value;

        public int Value
        {
            set => _value = value;
        }

        public int NotCompareValue { get; set; }
    }
}
