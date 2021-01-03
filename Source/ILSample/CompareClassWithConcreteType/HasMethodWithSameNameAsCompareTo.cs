using System;

namespace AssemblyToProcess.CompareClassWithConcreteType
{
    public class HasMethodWithSameNameAsCompareTo : IComparable
    {
        private readonly string _value;

        public HasMethodWithSameNameAsCompareTo(string value)
        {
            _value = value;
        }

        public int CompareTo(object obj)
        {
            return int.MaxValue;
        }


        int IComparable.CompareTo(object obj)
        {
            if (obj is null) return 1;

            if (!(obj is HasMethodWithSameNameAsCompareTo))
            {
                throw new ArgumentException("Object is not a HasMethodWithSameNameAsCompareTo");
            }
            return CompareTo((HasMethodWithSameNameAsCompareTo)obj);
        }

        public int CompareTo(HasMethodWithSameNameAsCompareTo obj)
        {
            if (obj is null) return 1;

            return _value.CompareTo(obj._value);
        }
    }
}
