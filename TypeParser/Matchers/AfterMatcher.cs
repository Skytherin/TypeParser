namespace TypeParser.Matchers
{
    internal class AfterMatcher : ITypeMatcher
    {
        private readonly string After;
        private readonly ITypeMatcher SubMatcher;

        public AfterMatcher(string after, ITypeMatcher subMatcher)
        {
            After = after;
            SubMatcher = subMatcher;
        }

        public bool TryScan(string input, out object? output, out string remainder)
        {
            var m = SubMatcher.TryScan(input, out output, out remainder);

            if (!m)
            {
                output = null;
                remainder = input;
                return false;
            }

            remainder = remainder.TrimStart();

            if (!remainder.StartsWith(After))
            {
                output = null;
                remainder = input;
                return false;
            }

            remainder = remainder[After.Length..];
            return true;
        }
    }
}