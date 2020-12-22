using Comparable;

// ReSharper disable UnusedMember.Global

namespace AssemblyToProcess.Class
{
    [Comparable]
    public class ClassProperty

    {
        [CompareBy]
        public string Value { get; set; }

        public int NotCompareValue { get; set; }
    }
}
