namespace TypeParser.Matchers
{
    internal class OptionalMatcher : ITypeMatcher
    {
        private readonly ITypeMatcher NonOptionalMatcher;

        public OptionalMatcher(ITypeMatcher nonOptionalMatcher)
        {
            NonOptionalMatcher = nonOptionalMatcher;
        }

        public ITypeMatcher.Result? Match(string input)
        {
            return NonOptionalMatcher.Match(input) ?? new(null, input);
        }
    }
}