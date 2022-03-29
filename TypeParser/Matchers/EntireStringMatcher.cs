namespace TypeParser.Matchers
{
    internal class EntireStringMatcher : ITypeMatcher
    {
        private readonly ITypeMatcher SubMatcher;

        public EntireStringMatcher(ITypeMatcher subMatcher)
        {
            SubMatcher = subMatcher;
        }

        public ITypeMatcher.Result? Match(string input)
        {
            input = input.TrimStart();
            var m = SubMatcher.Match(input);
            if (m == null) return null;
            var remainder = m.Remainder.Trim();
            if (remainder.Length > 0)
            {
                return null;
            }

            return new(m.Value, "");
        }
    }
}