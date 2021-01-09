using System;

namespace AssemblyNotToProcess
{
    public interface INestedComparable : IComparable, IComparable<INestedComparable>
    {
        string Value { get; set; }
    }
}