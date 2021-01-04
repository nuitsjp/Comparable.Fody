using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.CompareStructWithConcreteType
{
    [Comparable]
    public struct StructField

    {
        [CompareBy]
        private CompareStructWithConcreteTypeValue _value;

        // ReSharper disable once ConvertToAutoProperty
        public CompareStructWithConcreteTypeValue Value
        {
            get => _value;
            set => _value = value;
        }

        public int NotCompareValue { get; set; }
    }
}
