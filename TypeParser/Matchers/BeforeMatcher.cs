namespace TypeParser.Matchers
{
    internal class BeforeMatcher : ITypeMatcher
    {
        private readonly string Before;
        private readonly ITypeMatcher SubMatcher;

        public BeforeMatcher(string before, ITypeMatcher subMatcher)
        {
            Before = before;
            SubMatcher = subMatcher;
        }

        public bool TryScan(string input, out object? output, out string remainder)
        {
            if (!input.StartsWith(Before))
            {
                output = null;
                remainder = input;
                return false;
            }
            input = input[Before.Length..].TrimStart();

            return SubMatcher.TryScan(input, out output, out remainder);
        }
    }
}