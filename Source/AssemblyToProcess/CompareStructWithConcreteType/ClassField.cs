using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.CompareStructWithConcreteType
{
    [Comparable]
    public struct ClassField

    {
        [CompareBy]
        private CompareClassWithConcreteTypeValue _value;

        // ReSharper disable once ConvertToAutoProperty
        public CompareClassWithConcreteTypeValue Value
        {
            get => _value;
            set => _value = value;
        }

        public int NotCompareValue { get; set; }
    }
}
