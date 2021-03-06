﻿using System;

namespace AssemblyToProcess
{
    public class CompareClassWithConcreteTypeValue : IComparable, IComparable<CompareClassWithConcreteTypeValue>
    {
        public string Value { get; set; }
        
        public int CompareTo(object obj)
        {
            throw new InvalidOperationException();
        }

        public int CompareTo(CompareClassWithConcreteTypeValue obj)
        {
            if (obj is null) return 1;

            // ReSharper disable once StringCompareToIsCultureSpecific
            return Value.CompareTo(obj.Value);
        }

        public static implicit operator CompareClassWithConcreteTypeValue(string value)
        {
            return new() { Value = value };
        }

        public static implicit operator CompareClassWithConcreteTypeValue(int value)
        {
            return new() { Value = value.ToString() };
        }
    }
}