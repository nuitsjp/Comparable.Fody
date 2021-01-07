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
    public class CompareGenericClassField
    {
        [CompareBy]
        private IComparable _value;

        public IComparable Value
        {
            get => _value;
            set => _value = value;
        }
    }

    public interface IComparableValue : IComparable { }
}