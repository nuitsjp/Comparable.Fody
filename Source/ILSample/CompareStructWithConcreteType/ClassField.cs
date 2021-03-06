﻿using System;

namespace AssemblyToProcess.CompareStructWithConcreteType
{
    public readonly struct ClassField : IComparable
    {
        private readonly string _value;

        // ReSharper disable once UnusedMember.Global
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
            return CompareTo((ClassField)obj);
        }

        public int CompareTo(ClassField obj)
        {
            // ReSharper disable once StringCompareToIsCultureSpecific
            return _value.CompareTo(obj._value);
        }
    }
}
