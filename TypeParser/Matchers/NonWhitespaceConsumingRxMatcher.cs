using System.Text.RegularExpressions;

namespace TypeParser.Matchers
{
    internal class PlainRxMatcher: ITypeMatcher
    {
        private readonly Regex Rx;
        public PlainRxMatcher(Regex rx)
        {
            Rx = new($"^{rx}", RegexOptions.IgnoreCase);
        }

        public ITypeMatcher.Result? Match(string input)
        {
            var m = Rx.Match(input);
            if (!m.Success) return null;
            return new(m.Value, input[m.Length..]);
        }
    }
}