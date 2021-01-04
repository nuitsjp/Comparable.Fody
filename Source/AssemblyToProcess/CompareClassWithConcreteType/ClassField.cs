using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.CompareClassWithConcreteType
{
    [Comparable]
    public class ClassField

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
