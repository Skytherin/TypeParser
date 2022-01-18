using System.Text.RegularExpressions;

namespace TypeParser.Matchers
{
    internal class BeforeMatcher : ITypeMatcher
    {
        private readonly Regex Before;
        private readonly ITypeMatcher SubMatcher;

        public BeforeMatcher(Regex before, ITypeMatcher subMatcher)
        {
            Before = before;
            SubMatcher = subMatcher;
        }

        public ITypeMatcher.Result? Match(string input)
        {
            input = input.TrimStart();
            var m = Before.Match(input);
            if (!m.Success)
            {
                return null;
            }

            return SubMatcher.Match(input[m.Length..]);
        }
    }
}