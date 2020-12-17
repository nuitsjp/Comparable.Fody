using System;
using Comparable;

// ReSharper disable once CheckNamespace
public class WithSingleProperty
{
    public int Value { get; set; }

    public int CompareTo2(object obj)
    {
        return 1;
    }
}
