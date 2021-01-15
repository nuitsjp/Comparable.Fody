using System;
using Comparable;

namespace CompareToByGenericAlreadyExists
{
    [Comparable]
    public class CompareToByGenericAlreadyExists
    {
        [CompareBy]
        public int Value { get; set; }

        public int CompareTo(CompareToByGenericAlreadyExists other)
        {
            return 0;
        }
    }
}
