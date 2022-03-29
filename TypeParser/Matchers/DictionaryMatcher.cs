using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TypeParser.Matchers
{
    internal class DictionaryMatcher<TKey, TValue>: ITypeMatcher where TKey : notnull
    {
        private readonly ITypeMatcher KeyMatcher;
        private readonly ITypeMatcher ValueMatcher;
        private readonly InternalFormat Format;

        public DictionaryMatcher(ITypeMatcher keyMatcher, ITypeMatcher valueMatcher, InternalFormat format)
        {
            KeyMatcher = keyMatcher;
            ValueMatcher = valueMatcher;
            Format = format;
        }

        public ITypeMatcher.Result? Match(string input)
        {
            var result = new Dictionary<TKey, TValue>();
            var valueSeparator = new TypedRxMatcher<string>(new Regex(":"), it => it);
            var itemSeparator = Format.Separator == null ? null : new TypedRxMatcher<string>(Format.Separator, it => it);
            while (true)
            {
                if (KeyMatcher.TryMatch(input, out var key, out input) &&
                    valueSeparator.TryMatch(input, out _, out input) &&
                    ValueMatcher.TryMatch(input, out var value, out input))
                {
                    result.Add((TKey)key!, (TValue)value!);
                }
                else
                {
                    return null;
                }

                if (string.IsNullOrWhiteSpace(input)) return new(result, "");

                if (itemSeparator == null)
                {
                    input = input.TrimStart();
                    continue;
                }

                if (itemSeparator.TryMatch(input, out _, out input)) continue;
                return new(result, input);
            }
        }
    }
}