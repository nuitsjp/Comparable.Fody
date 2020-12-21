using System;

namespace ILSample
{
    public readonly struct StructWithSingleField : IComparable
    {
        private readonly int _value;

        public int CompareTo(object obj)
        {
            if (obj is null) return 1;

            if (!(obj is StructWithSingleField))
            {
                throw new ArgumentException("Object is not a StructWithSingleField");
            }
            var comparable = (StructWithSingleField)obj;

            return _value.CompareTo(comparable._value);
        }
    }
}
