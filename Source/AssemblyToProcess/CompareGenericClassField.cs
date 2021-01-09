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