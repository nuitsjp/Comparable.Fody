﻿using System;

namespace AssemblyToProcess.CompareClassWithObject
{
    public class ClassField : IComparable
    {
        private readonly ClassValue _value;

        public ClassField(ClassValue value)
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
            if (obj is null) return 1;

            return _value.CompareTo(obj._value);
        }
    }
}
