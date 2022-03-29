using System;
using System.Text.RegularExpressions;

namespace TypeParser.Matchers
{
    internal class IntMatcher : TypedRxMatcher<int>
    {
        public IntMatcher(Regex? regex) : base(regex ?? new(@"-?\d+"), Convert.ToInt32)
        {
        }
    }
}