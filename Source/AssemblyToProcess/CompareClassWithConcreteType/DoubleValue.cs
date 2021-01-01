using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.CompareClassWithConcreteType
{
    [Comparable]
    public class DoubleValue
    {
        [CompareBy(Priority = 2)]
        private string _value1;

        [CompareBy]
        public int Value0 { get; set; }

        // ReSharper disable once ConvertToAutoProperty
        public string Value1
        {
            get => _value1;
            set => _value1 = value;
        }

        public int NotCompareValue { get; set; }
    }
}
