using Comparable;
// ReSharper disable UnusedMember.Global

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
