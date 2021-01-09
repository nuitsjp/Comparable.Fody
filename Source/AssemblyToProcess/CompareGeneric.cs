using AssemblyNotToProcess;
using Comparable;
// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess
{
    [Comparable]
    public class CompareGenericClassField<T> where T : INestedComparable
    {
        [field: CompareBy]
        public T Value { get; set; }
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