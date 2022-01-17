using System;
using System.Collections.Generic;
using System.Linq;
using Common.Utils;
using JetBrains.Annotations;
using TypeParser.Matchers;

namespace TypeParser
{
    [UsedImplicitly]
    public static class TypeParser
    {
        [UsedImplicitly]
        public static ITypeMatcher Compile<T>(RxFormat? format = null, RxRepeat? repeat = null) 
            => new EntireStringMatcher(TypeMatcherHelper.TypeParserForType(typeof(T), format, repeat));

        public static T Parse<T>(string input)
        {
            var compiled = Compile<T>();
            if (compiled.TryScan(input, out var output, out _))
            {
                return (T)output!;
            }

            throw new ApplicationException("Failed to match.");
        }

        [UsedImplicitly]
        public static List<T> ParseLines<T>(string input)
        {
            return input.Lines().Select(Parse<T>).ToList();
        }

        [UsedImplicitly]
        public static T? ParseOrDefault<T>(string input)
        {
            try
            {
                return Parse<T>(input);
            }
            catch
            {
                return default;
            }
        }
    }
}