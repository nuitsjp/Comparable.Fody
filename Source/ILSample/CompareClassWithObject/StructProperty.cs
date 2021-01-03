using System;

namespace AssemblyToProcess.CompareClassWithObject
{
    public class StructProperty : IComparable
    {
        public StructValue Value { get; set; }

        public int CompareTo(object value)
        {
            if (value is null) return 1;

            if (!(value is StructProperty))
            {
                throw new ArgumentException("Object is not a StructProperty");
            }
            return CompareTo((StructProperty)value);
        }

        public int CompareTo(StructProperty obj)
        {
            if (obj is null) return 1;

            return Value.CompareTo(obj.Value);
        }
    }
}
