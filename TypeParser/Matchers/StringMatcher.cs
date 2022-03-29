using System.Text.RegularExpressions;

namespace TypeParser.Matchers
{
    internal class StringMatcher : TypedRxMatcher<string>
    {
        public StringMatcher(Regex? regex) : base(regex ?? new(@"[a-zA-Z]+"), s => s)
        {
        }
    }
}