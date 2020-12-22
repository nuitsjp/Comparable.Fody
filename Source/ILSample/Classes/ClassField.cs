using System;

namespace ILSample.Classes
{
    public class ClassField : IComparable
    {
        private readonly string _value;

        public ClassField(string value)
        {
            _value = value;
        }

        public int CompareTo(object obj)
        {
            if (obj is null) return 1;

            if (!(obj is ClassField))
            {
                throw new ArgumentException("Object is not a ClassField");
            }
            var comparable = (ClassField)obj;

            return _value.CompareTo(comparable._value);
        }
    }
}
