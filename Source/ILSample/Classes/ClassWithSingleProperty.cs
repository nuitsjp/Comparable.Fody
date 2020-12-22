using System;

namespace ILSample.Classes
{
    public class ClassWithSingleProperty : IComparable
    {
        public string Value { get; set; }

        public int CompareTo(object obj)
        {
            if (obj is null) return 1;

            var comparable = obj as ClassWithSingleProperty;
            if (comparable is null)
            {
                throw new ArgumentException("Object is not a ClassWithSingleProperty");
            }

            return Value.CompareTo(comparable.Value);
        }
    }
}
