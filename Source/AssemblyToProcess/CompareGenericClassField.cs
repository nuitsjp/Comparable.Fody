using AssemblyNotToProcess;
using Comparable;

namespace AssemblyToProcess
{
    [Comparable]
    public class CompareGenericClassField<T> where T : INestedComparable
    {
        [CompareBy] private T _value;

        
        public T Value
        {
            get => _value;
            set => _value = value;
        }

        public int CompareToX(CompareGenericClassField<T> other)
        {
            //if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;

            return _value.CompareTo(other._value);
        }
    }

    [Comparable]
    public class CompareGenericClassProperty<T> where T : INestedComparable
    {
        [CompareBy]
        public T Value { get; set; }
    }


    [Comparable]
    public struct CompareGenericStructField<T> where T : INestedComparable
    {
        [field: CompareBy]
        public T Value { get; set; }
    }

    [Comparable]
    public struct CompareGenericStructProperty<T> where T : INestedComparable
    {
        [CompareBy]
        public T Value { get; set; }
    }
}