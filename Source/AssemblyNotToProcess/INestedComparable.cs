using System;

namespace AssemblyNotToProcess
{
    public interface INestedComparable : IComparable, IComparable<INestedComparable>
    {
        string Value { get; set; }
    }


    public interface INestedComparableA : IComparable, IComparable<INestedComparableA> { }
    public interface INestedComparableB : IComparable, IComparable<INestedComparableB> { }

    public interface INestedComparableAb : INestedComparableA, INestedComparableB { }


    public class NestedComparableAb : INestedComparableAb
    {
        public int CompareTo(object obj) => throw new NotImplementedException();

        public int CompareTo(INestedComparableA other) => throw new NotImplementedException();

        public int CompareTo(INestedComparableB other) => throw new NotImplementedException();
    }
}