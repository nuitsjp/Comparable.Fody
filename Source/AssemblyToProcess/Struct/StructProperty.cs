﻿using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.Struct
{
    [Comparable]
    public struct StructProperty

    {
        [CompareBy]
        public int Value { get; set; }

        public int NotCompareValue { get; set; }
    }
}
