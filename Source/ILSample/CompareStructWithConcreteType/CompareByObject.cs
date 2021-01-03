using System;

namespace AssemblyToProcess.CompareStructWithConcreteType
{
    public struct CompareByObject : IComparable
    {
        public CompareByObjectValue Value { get; set; }
        public int CompareTo(object obj)
        {
            return Value.CompareTo(((CompareByObject) obj).Value);
        }

        public int CompareTo(CompareByObject obj)
        {
            return Value.CompareTo(obj.Value);
        }
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