using System;

namespace AssemblyToProcess
{
    // ReSharper disable once UnusedMember.Global
    public class SimpleComparable : IComparable
    {
        public int CompareTo(object obj)
        {
            return 1;
        }
    }
}