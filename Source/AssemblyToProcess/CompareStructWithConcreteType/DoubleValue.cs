﻿using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.CompareStructWithConcreteType
{
    [Comparable]
    public struct DoubleValue
    {
        [CompareBy(Priority = 2)]
        private CompareClassWithConcreteTypeValue _value1;

        [CompareBy]
        public int Value0 { get; set; }

        // ReSharper disable once ConvertToAutoProperty
        public CompareClassWithConcreteTypeValue Value1
        {
            get => _value1;
            set => _value1 = value;
        }

        public int NotCompareValue { get; set; }
    }
}
