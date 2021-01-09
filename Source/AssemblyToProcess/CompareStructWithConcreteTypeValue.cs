using System;

namespace AssemblyToProcess
{
    public struct CompareStructWithConcreteTypeValue : IComparable, IComparable<CompareStructWithConcreteTypeValue>
    {
        public int Value { get; set; }

        public int CompareTo(object obj)
        {
            throw new InvalidOperationException();
        }

        public int CompareTo(CompareStructWithConcreteTypeValue obj)
        {
            return Value.CompareTo(obj.Value);
        }

        public static implicit operator CompareStructWithConcreteTypeValue(int value)
        {
            return new() { Value = value };
        }
    }
}