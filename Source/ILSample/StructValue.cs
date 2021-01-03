using System;

namespace AssemblyToProcess
{
    public struct StructValue : IComparable
    {
        public int Value { get; set; }

        public int CompareTo(object obj)
        {
            if (obj is null) return 1;

            if (obj is StructValue classValue)
            {
                return Value.CompareTo(classValue.Value);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public static implicit operator StructValue(int value)
        {
            return new() { Value = value };
        }
    }
}