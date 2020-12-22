using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.Struct
{
    [Comparable]
    public struct StructField

    {
        [CompareBy]
        private int _value;

        // ReSharper disable once ConvertToAutoProperty
        public int Value
        {
            get => _value;
            set => _value = value;
        }

        public int NotCompareValue { get; set; }
    }
}
