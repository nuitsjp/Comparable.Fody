﻿using Comparable;
// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess
{
    [Comparable]
    public class WithSingleProperty
    {
        [CompareBy(Priority = 1)]
        public int Value { get; set; }
        public int NotCompareValue { get; set; }
    }
}
