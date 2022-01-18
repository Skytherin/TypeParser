using System.Text.RegularExpressions;

namespace TypeParser.Matchers
{
    internal class StringMatcher : RxMatcher<string>
    {
        public StringMatcher(Regex? regex) : base(regex ?? new(@"[a-zA-Z]+"), s => s)
        {
        }
    }
}