namespace TypeParser.Matchers
{
    internal class OptionalMatcher: ITypeMatcher
    {
        private readonly ITypeMatcher NonOptionalMatcher;

        public OptionalMatcher(ITypeMatcher nonOptionalMatcher)
        {
            NonOptionalMatcher = nonOptionalMatcher;
        }

        public bool TryScan(string input, out object? output, out string remainder)
        {
            if (NonOptionalMatcher.TryScan(input, out output, out remainder))
            {
                return true;
            }

            output = null;
            remainder = input;
            return true;
        }
    }
}