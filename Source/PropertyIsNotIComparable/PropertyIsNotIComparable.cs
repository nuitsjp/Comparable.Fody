using System;
using Comparable;

namespace PropertyIsNotIComparable
{
    [Comparable]
    public class PropertyIsNotIComparable
    {
        [CompareBy]
        public NotComparable Value { get; set; }
    }
    
    public class NotComparable { }
}
