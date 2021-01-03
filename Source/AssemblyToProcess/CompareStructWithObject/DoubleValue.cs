using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.CompareStructWithObject
{
    [Comparable]
    public struct DoubleValue
    {
        [CompareBy(Priority = 2)]
        private ClassValue _value1;

        [CompareBy]
        public StructValue Value0 { get; set; }

        // ReSharper disable once ConvertToAutoProperty
        public ClassValue Value1
        {
            get => _value1;
            set => _value1 = value;
        }

        public int NotCompareValue { get; set; }
    }
}
