using System;
using Comparable;

namespace AssemblyToProcess.CompareStructWithConcreteType
{
    [Comparable]
    public struct CompareByObject
    {
        [CompareBy]
        public CompareByObjectValue Value { get; set; }
    }

    public struct CompareByObjectValue : IComparable
    {
        public int Value { get; set; }
        public int CompareTo(object obj)
        {
            return Value.CompareTo(((CompareByObjectValue)obj).Value);
        }
    }
}