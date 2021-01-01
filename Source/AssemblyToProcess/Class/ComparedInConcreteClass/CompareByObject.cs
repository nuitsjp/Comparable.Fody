using System;
using Comparable;

namespace AssemblyToProcess.Class.ComparedInConcreteClass
{
    [Comparable]
    public class CompareByObject
    {
        [CompareBy]
        public CompareByObjectValue Value { get; set; }
    }

    public class CompareByObjectValue : IComparable
    {
        public int Value { get; set; }
        public int CompareTo(object obj)
        {
            return Value.CompareTo(((CompareByObjectValue)obj).Value);
        }
    }
}