using System;

namespace AssemblyToProcess.CompareClassWithConcreteType
{
    public class GenericField<T> : IComparable, IComparable<GenericField<T>> where T : IComparable
    {
        private T _value;

        public T Value
        {
            get => _value;
            set => _value = value;
        }

        public int CompareTo(object other)
        {
            //if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;

            if (other is not GenericField<T>)
            {
                throw new ArgumentException();
            }

            return CompareTo((GenericField<T>) other);
        }

        public int CompareTo(GenericField<T> other)
        {
            //if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;

            return _value.CompareTo(other._value);
        }
    }

    public interface IComparableValue : IComparable { }
}