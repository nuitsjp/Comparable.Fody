using System;

namespace ILSample.Structs
{
    public readonly struct StructField : IComparable
    {
        private readonly int _value;

        public int CompareTo(object obj)
        {
            if (obj is null) return 1;

            if (!(obj is StructField))
            {
                throw new ArgumentException("Object is not a StructField");
            }
            var comparable = (StructField)obj;

            return _value.CompareTo(comparable._value);
        }
    }
}
