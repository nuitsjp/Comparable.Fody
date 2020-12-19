using System;

namespace Comparable
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class CompareByAttribute : Attribute
    {
        public const int DefaultPriority = 0;

        public CompareByAttribute(int priority = DefaultPriority)
        {
            Priority = priority;
        }
        
        /// <summary>
        /// Priority in CompareTo.
        /// </summary>
        // ReSharper disable once MemberInitializerValueIgnored
        public int Priority { get; set; } = DefaultPriority;
    }
}