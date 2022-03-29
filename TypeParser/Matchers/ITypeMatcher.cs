namespace TypeParser.Matchers
{
    internal interface ITypeMatcher
    {
        record Result(object? Value, string Remainder);
        Result? Match(string input);
    }

    internal static class TypeMatcherExtensions
    {
        public static bool TryMatch(this ITypeMatcher matcher, string input, out object? result, out string remainder)
        {
            var temp = matcher.Match(input);
            if (temp == null)
            {
                result = null;
                remainder = input;
                return false;
            }

            result = temp.Value;
            remainder = temp.Remainder;
            return true;
        }
    }
}