using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.CompareStructWithObject
{
    [Comparable]
    public struct ClassField

    {
        [CompareBy]
        private ClassValue _value;

        // ReSharper disable once ConvertToAutoProperty
        public ClassValue Value
        {
            get => _value;
            set => _value = value;
        }

        public int NotCompareValue { get; set; }
    }
}
