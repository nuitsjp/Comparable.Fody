using System;

namespace AssemblyToProcess.CompareClassWithObject
{
    public class StructField : IComparable
    {
        private readonly StructValue _value;

        public int CompareTo(object value)
        {
            if (value is null) return 1;

            if (!(value is StructField))
            {
                throw new ArgumentException("Object is not a StructField");
            }
            return CompareTo((StructField)value);
        }

        public int CompareTo(StructField obj)
        {
            if (obj is null) return 1;

            return _value.CompareTo(obj._value);
        }
    }
}
