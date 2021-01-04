using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.CompareClassWithConcreteType
{
    [Comparable]
    public class StructField

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
