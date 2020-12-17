using System;

namespace ILSample
{
    public class WithSingleProperty : IComparable
    {
        public int Value { get; set; }

        public int CompareTo(object obj)
        {
            if (obj is null) return 1;

            var comparable = obj as WithSingleProperty;
            if (comparable is null)
            {
                throw new ArgumentException("Object is not a WithSingleProperty");
            }

            return Value.CompareTo(comparable.Value);
        }
    }
}
