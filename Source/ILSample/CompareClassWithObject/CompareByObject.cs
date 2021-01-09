using System;

namespace AssemblyToProcess.CompareClassWithObject
{
    public class CompareByObject : IComparable, IComparable<CompareByObject>
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

    public class CompareByObjectValue : IComparable
    {
        public int Value { get; set; }
        public int CompareTo(object obj)
        {
            return Value.CompareTo(((CompareByObjectValue)obj).Value);
        }
    }
}