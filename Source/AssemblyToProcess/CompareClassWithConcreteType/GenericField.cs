//using System;
//using Comparable;

//namespace AssemblyToProcess.CompareClassWithConcreteType
//{
//    [Comparable]
//    public class GenericField<T> where T : CompareClassWithConcreteTypeValue, IComparableValue
//    {
//        [CompareBy]
//        private T _value;

//        public T Value
//        {
//            get => _value;
//            set => _value = value;
//        }
//    }

//    public interface IComparableValue : IComparable { }
//}