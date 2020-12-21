using System;

namespace ILSample
{
    public class ClassWithSingleProperty : IComparable
    {
        public int Value { get; set; }

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
