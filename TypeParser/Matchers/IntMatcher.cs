using System;

namespace TypeParser.Matchers
{
    internal class IntMatcher : RxMatcher
    {
        public IntMatcher() : base(@"-?\d+", s => Convert.ToInt32(s))
        {
        }
    }
}