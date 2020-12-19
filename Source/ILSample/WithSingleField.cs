using System;

namespace ILSample
{
    public class WithSingleField : IComparable
    {
        private readonly int _value;

        public int CompareTo(object obj)
        {
            if (obj is null) return 1;

            var comparable = obj as WithSingleField;
            if (comparable is null)
            {
                throw new ArgumentException("Object is not a WithSingleProperty");
            }

            return _value.CompareTo(comparable._value);
        }
    }
}
