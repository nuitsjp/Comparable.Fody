using System;

namespace ILSample
{
    public class WithSingleProperty : IComparable
    {
        public int Value { get; set; }

        public int CompareTo(object obj)
        {
            return 0;
            //if (obj is null) return 1;

            //if (obj is WithSingleProperty withSingleProperty)
            //{
            //    return Value.CompareTo(withSingleProperty.Value);
            //}

            //throw new ArgumentException("Object is not a WithSingleProperty");
        }
    }
}
