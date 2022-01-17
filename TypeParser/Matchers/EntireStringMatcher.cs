namespace TypeParser.Matchers
{
    internal class EntireStringMatcher : ITypeMatcher
    {
        private readonly ITypeMatcher Internals;

        public EntireStringMatcher(ITypeMatcher internals)
        {
            Internals = internals;
        }

        public bool TryScan(string input, out object? output, out string remainder)
        {
            input = input.TrimStart();
            if (!Internals.TryScan(input, out output, out remainder)) return false;
            remainder = remainder.Trim();
            if (remainder.Length > 0)
            {
                return false;
            }

            return true;
        }
    }
}