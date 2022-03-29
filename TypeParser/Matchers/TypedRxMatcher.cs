using System;
using System.Text.RegularExpressions;

namespace TypeParser.Matchers
{
    internal class TypedRxMatcher<T>: ITypeMatcher
    {
        private readonly PlainRxMatcher Rx;
        private readonly Func<string, T> Convert;

        internal TypedRxMatcher(Regex rx, Func<string, T> convert)
        {
            Rx = new PlainRxMatcher(rx);
            Convert = convert;
        }

        public ITypeMatcher.Result? Match(string input)
        {
            input = input.TrimStart();
            var m = Rx.Match(input);
            if (m == null) return null;
            return new(Convert((string)m.Value!), m.Remainder);
        }
    }
}