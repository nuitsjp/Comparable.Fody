using System;

namespace AssemblyNotToProcess
{
    public class ValueStruct : INestedComparable
    {
        public string Value { get; set; }
        public int CompareTo(object obj)
        {
            if (obj is null) return 1;

            if (obj is ValueStruct classValue)
            {
                return string.Compare(Value, classValue.Value, StringComparison.Ordinal);
            }

            throw new ArgumentException();
        }

        public static implicit operator ValueStruct(int value)
        {
            return new() { Value = value.ToString() };
        }

        public int CompareTo(INestedComparable other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return string.Compare(Value, other.Value, StringComparison.Ordinal);
        }
    }
}
