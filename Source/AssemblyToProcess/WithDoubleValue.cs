using System;
using Comparable;

namespace AssemblyToProcess
{
    [Comparable]
    public class WithDoubleValue
    {
        [CompareBy(Priority = 2)]
        private string _value1;

        [CompareBy]
        public int Value0 { get; set; }

        public string Value1
        {
            get => _value1;
            set => _value1 = value;
        }

        public int NotCompareValue { get; set; }
    }
}
