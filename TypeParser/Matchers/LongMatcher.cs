using System;
using System.Text.RegularExpressions;

namespace TypeParser.Matchers
{
    internal class LongMatcher : RxMatcher<long>
    {
        public LongMatcher(Regex? regex) : base(regex ?? new(@"-?\d+"), Convert.ToInt64)
        {
        }
    }
}