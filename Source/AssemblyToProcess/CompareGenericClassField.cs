using System;
using Comparable;

namespace AssemblyToProcess
{
    [Comparable]
    public class CompareGenericClassField<T> where T : IComparable
    {
        [field: CompareBy]
        public T Value { get; set; }
    }

    [Comparable]
    public class CompareGenericClassProperty<T> where T : IComparable
    {
        [CompareBy]
        public T Value { get; set; }
    }


    [Comparable]
    public struct CompareGenericStructField<T> where T : IComparable
    {
        [field: CompareBy]
        public T Value { get; set; }
    }

    [Comparable]
    public struct CompareGenericStructProperty<T> where T : IComparable
    {
        [CompareBy]
        public T Value { get; set; }
    }


    public interface IComparableValue : IComparable { }
}