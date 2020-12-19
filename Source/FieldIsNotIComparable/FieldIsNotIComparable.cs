using System;
using Comparable;

namespace FieldIsNotIComparable
{
    [Comparable]
    public class FieldIsNotIComparable
    {
        [CompareBy] 
#pragma warning disable 169
        private NotComparable _value;
#pragma warning restore 169
    }

    public class NotComparable { }
}
