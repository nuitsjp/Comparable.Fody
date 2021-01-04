using System.Collections.Generic;
using System.Linq;

namespace Comparable.Fody
{
    public static class EnumerableExtensions
    {
        public static bool Empty<TSource>(this IEnumerable<TSource> source)
        {
            return !source.Any();
        }
    }
}