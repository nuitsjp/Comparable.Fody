using System;
using Comparable;

namespace PropertyIsNotIComparable
{
    [Comparable.Comparable]
    public class PropertyIsNotIComparable
    {
        [CompareBy]
        public NotComparable Value { get; set; }
    }
    
    public class NotComparable { }
}
