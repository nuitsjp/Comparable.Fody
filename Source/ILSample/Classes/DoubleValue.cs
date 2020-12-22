using System;

namespace ILSample.Classes
{
    public class DoubleValue : IComparable
    {
        private readonly int _value0;

        public int Value1 { get; set; }

        public int CompareTo(object obj)
        {
            if (obj is null) return 1;

            if (!(obj is DoubleValue))
            {
                throw new ArgumentException("Object is not a DoubleValue");
            }
            var comparable = (DoubleValue)obj;

            var compared = _value0.CompareTo(comparable._value0);
            if (compared != 0) return compared;

            return Value1.CompareTo(comparable.Value1);
        }
    }
}
