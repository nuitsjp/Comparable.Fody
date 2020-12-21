using Comparable;
// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess
{
    [Comparable]
    public struct StructWithClassField

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
