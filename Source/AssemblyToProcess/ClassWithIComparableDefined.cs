﻿using Comparable;
// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess
{
    [Comparable]
    public class ClassWithIComparableDefined
    {
        [CompareBy]
        public int Value { get; set; }
    }
}