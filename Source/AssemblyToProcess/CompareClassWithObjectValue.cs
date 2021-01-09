using System;

namespace AssemblyToProcess
{
    public class CompareClassWithObjectValue : IComparable
    {
        public string Value { get; set; }
        
        public int CompareTo(object obj)
        {
            if (obj is null) return 1;

            if (obj is CompareClassWithObjectValue classValue)
            {
                // ReSharper disable once StringCompareToIsCultureSpecific
                return Value.CompareTo(classValue.Value);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public static implicit operator CompareClassWithObjectValue(string value)
        {
            return new() { Value = value };
        }

        public static implicit operator CompareClassWithObjectValue(int value)
        {
            return new() { Value = value.ToString() };
        }
    }
}