using System;
using AssemblyNotToProcess;

namespace AssemblyToProcess
{
    public class CompareGenericClassProperty<T> : IComparable, IComparable<CompareGenericClassProperty<T>> where T : INestedComparable
    {
        public T Value { get; set; }

        public int CompareTo(object other)
        {
            //if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;

            if (other is not CompareGenericClassProperty<T>)
            {
                throw new ArgumentException();
            }

            return CompareTo((CompareGenericClassProperty<T>) other);
        }

        public int CompareTo(CompareGenericClassProperty<T> other)
        {
            //if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;

            return Value.CompareTo(other.Value);
        }
    }

    public class CompareGenericClassField<T> : IComparable, IComparable<CompareGenericClassField<T>> where T : INestedComparable
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

            if (other is not CompareGenericClassField<T>)
            {
                throw new ArgumentException();
            }

            return CompareTo((CompareGenericClassField<T>)other);
        }

        public int CompareTo(CompareGenericClassField<T> other)
        {
            //if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;

            return _value.CompareTo(other._value);
        }
    }


    public struct CompareGenericStructProperty<T> : IComparable, IComparable<CompareGenericStructProperty<T>> where T : INestedComparable
    {
        public T Value { get; set; }

        public int CompareTo(object other)
        {
            //if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;

            if (other is not CompareGenericStructProperty<T>)
            {
                throw new ArgumentException();
            }

            return CompareTo((CompareGenericStructProperty<T>)other);
        }

        public int CompareTo(CompareGenericStructProperty<T> other)
        {
            return Value.CompareTo(other.Value);
        }
    }

    public struct CompareGenericStructField<T> : IComparable, IComparable<CompareGenericStructField<T>> where T : INestedComparable
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

            if (other is not CompareGenericStructField<T>)
            {
                throw new ArgumentException();
            }

            return CompareTo((CompareGenericStructField<T>)other);
        }

        public int CompareTo(CompareGenericStructField<T> other)
        {
            return _value.CompareTo(other._value);
        }
    }

    public interface IComparableValue : IComparable { }
}