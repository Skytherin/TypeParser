using System;

namespace TypeParser.Matchers
{
    internal class LongMatcher : RxMatcher
    {
        public LongMatcher() : base(@"-?\d+", s => Convert.ToInt64(s))
        {
        }
    }
}