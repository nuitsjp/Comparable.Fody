﻿using System;

namespace ILSample.Structs
{
    public struct HasMethodWithSameNameAsCompareTo : IComparable
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
            return _value.CompareTo(obj._value);
        }
    }
}
