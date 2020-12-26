using System;

namespace ILSample.Classes
{
    public class StructProperty : IComparable
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
            if (obj is null) return 1;

            return Value.CompareTo(obj.Value);
        }
    }
}
