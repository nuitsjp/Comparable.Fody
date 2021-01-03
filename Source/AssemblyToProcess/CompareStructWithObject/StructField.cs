using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.CompareStructWithObject
{
    [Comparable]
    public struct StructField

    {
        [CompareBy]
        private StructValue _value;

        // ReSharper disable once ConvertToAutoProperty
        public StructValue Value
        {
            get => _value;
            set => _value = value;
        }

        public int NotCompareValue { get; set; }
    }
}
