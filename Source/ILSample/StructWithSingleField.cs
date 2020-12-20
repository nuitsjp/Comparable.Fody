using System;

namespace ILSample
{
    public struct StructWithSingleField : IComparable
    {
        private readonly int _value;

        public int CompareTo(object obj)
        {
            if (obj is null) return 1;

            if (obj is not StructWithSingleField)
            {
                throw new ArgumentException("Object is not a WithSingleProperty");
            }
            var comparable = (StructWithSingleField)obj;

            return _value.CompareTo(comparable._value);
        }
    }
}
