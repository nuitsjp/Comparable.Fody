using Comparable;

namespace FieldIsNotIComparable
{
    [Comparable]
    // ReSharper disable once UnusedMember.Global
    public class FieldIsNotIComparable
    {
        [CompareBy] 
#pragma warning disable 169
        private NotComparable _value;
#pragma warning restore 169
    }

    public class NotComparable { }
}
