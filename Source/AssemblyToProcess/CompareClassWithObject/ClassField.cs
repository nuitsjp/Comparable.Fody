using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.CompareClassWithObject
{
    [Comparable]
    public class ClassField

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
