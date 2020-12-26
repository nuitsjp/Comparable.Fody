﻿using System;

namespace ILSample.Structs
{
    public struct ClassProperty : IComparable
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
            return Value.CompareTo(obj.Value);
        }
    }
}
