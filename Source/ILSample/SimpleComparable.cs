using System;

namespace AssemblyToProcess
{
    public class SimpleComparable : IComparable
    {
        public int CompareTo(object obj)
        {
            return 1;
        }
    }
}