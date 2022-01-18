using System.Text.RegularExpressions;

namespace TypeParser.Matchers
{
    internal class CharMatcher : RxMatcher<char>
    {
        public CharMatcher(Regex? match) : base(match ?? new(@"\S"), s => s[0])
        {
        }
    }
}