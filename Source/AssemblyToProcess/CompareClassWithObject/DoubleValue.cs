using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.CompareClassWithObject
{
    [Comparable]
    public class DoubleValue
    {
        [CompareBy(Priority = 2)]
        private CompareClassWithObjectValue _value1;

        [CompareBy]
        public CompareStructWithObjectValue Value0 { get; set; }

        // ReSharper disable once ConvertToAutoProperty
        public CompareClassWithObjectValue Value1
        {
            get => _value1;
            set => _value1 = value;
        }

        public int NotCompareValue { get; set; }
    }
}
