﻿using Comparable;
// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess
{
    [Comparable]
    public class WithSingleField
    
    {
        [CompareBy]
        private int _value;

        // ReSharper disable once ConvertToAutoProperty
        public int Value
        {
            get => _value;
            set => _value = value;
        }

        public int NotCompareValue { get; set; }
    }
}
