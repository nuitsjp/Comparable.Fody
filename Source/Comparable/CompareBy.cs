using System;

namespace Comparable
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class CompareBy : Attribute
    {
        public const int DefaultPriority = 0;
        /// <summary>
        /// Priority in CompareTo.
        /// </summary>
        public int Priority { get; set; } = DefaultPriority;
    }
}