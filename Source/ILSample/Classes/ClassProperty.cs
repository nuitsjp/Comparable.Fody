using System;

namespace ILSample.Classes
{
    public class ClassProperty : IComparable
    {
        public string Value { get; set; }

        public int CompareTo(object obj)
        {
            if (obj is null) return 1;

            if (!(obj is ClassProperty))
            {
                throw new ArgumentException("Object is not a ClassProperty");
            }
            return CompareTo((ClassProperty)obj);
        }

        public int CompareTo(ClassProperty obj)
        {
            if (obj is null) return 1;

            return Value.CompareTo(obj.Value);
        }
    }
}
