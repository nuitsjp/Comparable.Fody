using System;

namespace AssemblyToProcess
{
    public class ClassValue : IComparable
    {
        public string Value { get; set; }
        
        public int CompareTo(object obj)
        {
            if (obj is null) return 1;

            if (obj is ClassValue classValue)
            {
                return Value.CompareTo(classValue.Value);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public static implicit operator ClassValue(string value)
        {
            return new() {Value = value};
        }
    }
}