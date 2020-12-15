using System;

namespace ILSample
{
    public class WithTowProperty : IComparable
    {
        public int Value0 { get; set; }

        public int Value1 { get; set; }

        public int CompareTo(object obj)
        {
            if (obj is null) return 1;

            if (obj is WithTowProperty withTowProperty)
            {
                var compared = Value0.CompareTo(withTowProperty.Value0);
                if (compared != 0) return compared;

                return Value1.CompareTo(withTowProperty.Value1);
            }

            throw new ArgumentException("Object is not a WithSingleProperty");
        }
    }
}
