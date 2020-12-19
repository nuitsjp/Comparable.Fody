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
            get => _value;
            set => _value = value;
        }

        public int NotCompareValue { get; set; }
    }
}
