using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.CompareClassWithObject
{
    [Comparable]
    public class StructField

    {
        [CompareBy]
        private CompareStructWithObjectValue _withObjectValue;

        // ReSharper disable once ConvertToAutoProperty
        public CompareStructWithObjectValue Value
        {
            get => _withObjectValue;
            set => _withObjectValue = value;
        }

        public int NotCompareValue { get; set; }
    }
}
