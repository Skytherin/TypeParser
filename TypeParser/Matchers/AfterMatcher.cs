using System.Text.RegularExpressions;

namespace TypeParser.Matchers
{
    internal class AfterMatcher : ITypeMatcher
    {
        private readonly Regex After;
        private readonly ITypeMatcher SubMatcher;

        public AfterMatcher(Regex after, ITypeMatcher subMatcher)
        {
            After = after;
            SubMatcher = subMatcher;
        }

        public ITypeMatcher.Result? Match(string input)
        {
            var m = SubMatcher.Match(input);

            if (m == null) return null;

            var remainder = m.Remainder.TrimStart();

            var m2 = After.Match(remainder);

            if (!m2.Success)
            {
                return null;
            }

            remainder = remainder[m2.Length..];
            return new(m.Value, remainder);
        }
    }
}