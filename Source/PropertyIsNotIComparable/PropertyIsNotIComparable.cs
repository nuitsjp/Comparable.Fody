using System;
using Comparable;

namespace PropertyIsNotIComparable
{
    [AddComparable]
    public class PropertyIsNotIComparable
    {
        [CompareBy]
        public NotComparable Value { get; set; }
    }
    
    public class NotComparable { }
}
