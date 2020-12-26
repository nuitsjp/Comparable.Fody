using System;

namespace ILSample.Structs
{
    public struct StructProperty : IComparable
    {
        public int Value { get; set; }

        public int CompareTo(object obj)
        {
            if (obj is null) return 1;

            if (!(obj is StructProperty))
            {
                throw new ArgumentException("Object is not a StructProperty");
            }
            return CompareTo((StructProperty)obj);
        }

        public int CompareTo(StructProperty obj)
        {
            return Value.CompareTo(obj.Value);
        }
    }
}
