using System.Text.RegularExpressions;

namespace TypeParser.Matchers
{
    internal class CharMatcher : TypedRxMatcher<char>
    {
        public CharMatcher(Regex? match) : base(match ?? new(@"\S"), s => s[0])
        {
        }
    }
}