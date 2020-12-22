﻿using System;

namespace ILSample.Structs
{
    public struct StructProperty : IComparable
    {
        public string Value { get; set; }

        public int CompareTo(object obj)
        {
            if (obj is null) return 1;

            if (!(obj is StructProperty))
            {
                throw new ArgumentException("Object is not a StructProperty");
            }
            var comparable = (StructProperty)obj;

            return Value.CompareTo(comparable.Value);
        }
    }
}