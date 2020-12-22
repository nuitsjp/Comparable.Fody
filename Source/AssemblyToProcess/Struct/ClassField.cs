using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.Struct
{
    [Comparable]
    public struct ClassField

    {
        [CompareBy]
        private string _value;

        // ReSharper disable once ConvertToAutoProperty
        public string Value
        {
            get => _value;
            set => _value = value;
        }

        public int NotCompareValue { get; set; }
    }
}
