﻿using System;

namespace ILSample
{
    public class WithDoubleProperty : IComparable
    {
        public int Value0 { get; set; }

        public int Value1 { get; set; }

        public int CompareTo(object obj)
        {
            if (obj is null) return 1;

            var comparable = obj as WithDoubleProperty;
            if (comparable is null)
            {
                throw new ArgumentException("Object is not a WithSingleProperty");
            }

            var compared = Value0.CompareTo(comparable.Value0);
            if (compared != 0) return compared;

            return Value1.CompareTo(comparable.Value1);
        }
    }
}
