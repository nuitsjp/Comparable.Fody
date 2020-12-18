using System;

namespace Comparable
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public class AddComparable : Attribute
    {
        public int Value { get; set; }
    }
}
