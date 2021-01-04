using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.CompareStructWithObject
{
    [Comparable]
    public struct ClassField

    {
        [CompareBy]
        private CompareClassWithObjectValue _value;

        // ReSharper disable once ConvertToAutoProperty
        public CompareClassWithObjectValue Value
        {
            get => _value;
            set => _value = value;
        }

        public int NotCompareValue { get; set; }
    }
}
