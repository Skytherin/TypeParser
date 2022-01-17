using System;
using System.Text.RegularExpressions;

namespace TypeParser.Matchers
{
    internal class RxMatcher: ITypeMatcher
    {
        private readonly string Rx;
        private readonly Func<string, object?> Convert;

        public RxMatcher(string rx, Func<string, object?> convert)
        {
            Rx = rx;
            Convert = convert;
        }

        public bool TryScan(string input, out object? output, out string remainder)
        {
            var m = Regex.Match(input, $@"^{Rx}");
            if (!m.Success)
            {
                output = default!;
                remainder = input;
                return false;
            }
            output = Convert(m.Value);
            remainder = input[m.Length..];
            return true;
        }
    }
}