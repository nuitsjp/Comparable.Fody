﻿using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.CompareStructWithObject
{
    [Comparable]
    public struct IsDefinedCompareAttribute
    {
        [CompareBy]
        public CompareStructWithObjectValue Value { get; set; }
    }
}