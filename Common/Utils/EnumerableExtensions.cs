using System.Collections.Generic;
using System.Linq;

namespace Common.Utils
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<(T Value, int Index)> WithIndices<T>(this IEnumerable<T> self) =>
            self.Select((it, index) => (it, index));
    }
}