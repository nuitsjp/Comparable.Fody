using System;
using Comparable;

namespace AssemblyToProcess
{
    [Comparable]
    public class CompareGenericClassField<T> where T : IComparable
    {
        [CompareBy]
        private T _value;

        public T Value
        {
            get => _value;
            set => _value = value;
        }
    }

    [Comparable]
    public struct CompareGenericStructField<T> where T : IComparable
    {
        [CompareBy]
        private T _value;

        public T Value
        {
            get => _value;
            set => _value = value;
        }
    }


    public interface IComparableValue : IComparable { }
}