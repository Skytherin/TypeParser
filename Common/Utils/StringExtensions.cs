using System.Collections.Generic;
using System.Linq;

namespace Common.Utils
{
    public static class StringExtensions
    {
        public static string Join<T>(this IEnumerable<T> self, string separator = "")
        {
            return string.Join(separator, self);
        }
        public static List<string> Lines(this string input) =>
            input.Split("\n")
                .Select(it => it.Trim()).ToList();

    }
}