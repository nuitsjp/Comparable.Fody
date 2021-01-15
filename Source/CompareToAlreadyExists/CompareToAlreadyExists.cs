using System;
using Comparable;

namespace CompareToAlreadyExists
{
    [Comparable]
    public class CompareToAlreadyExists
    {
        [CompareBy]
        public int Value { get; set; }

        public int CompareTo(object other)
        {
            return 0;
        }
    }
}
