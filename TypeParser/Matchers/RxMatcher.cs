using System;
using System.Text.RegularExpressions;

namespace TypeParser.Matchers
{
    internal class RxMatcher<T>: ITypeMatcher
    {
        private readonly Regex Rx;
        private readonly Func<string, T> Convert;

        public RxMatcher(Regex rx, Func<string, T> convert)
        {
            Rx = new($"^{rx}");
            Convert = convert;
        }

        public ITypeMatcher.Result? Match(string input)
        {
            input = input.TrimStart();
            var m = Rx.Match(input);
            if (!m.Success) return null;
            return new(Convert(m.Value), input[m.Length..]);
        }
    }
}