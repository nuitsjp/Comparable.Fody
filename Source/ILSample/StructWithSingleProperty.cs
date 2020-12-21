using System;

namespace ILSample
{
    public struct StructWithSingleProperty : IComparable
    {
        public string Value { get; set; }

        public int CompareTo(object obj)
        {
            if (obj is null) return 1;

            if (!(obj is StructWithSingleProperty))
            {
                throw new ArgumentException("Object is not a StructWithSingleProperty");
            }
            var comparable = (StructWithSingleProperty)obj;

            return Value.CompareTo(comparable.Value);
        }
    }
}
