﻿using System;
// ReSharper disable UnusedMember.Global

namespace ILSample
{
    public class WithTripleProperty : IComparable
    {
        public int Value0 { get; set; }

        public int Value1 { get; set; }
        public int Value2 { get; set; }

        public int CompareTo(object obj)
        {
            if (obj is null) return 1;

            var comparable = obj as WithTripleProperty;
            if (comparable is null)
            {
                throw new ArgumentException("Object is not a WithSingleProperty");
            }

            var compared = Value0.CompareTo(comparable.Value0);
            if (compared != 0) return compared;

            compared = Value1.CompareTo(comparable.Value1);
            if (compared != 0) return compared;

            return Value2.CompareTo(comparable.Value2);
        }
    }
}
