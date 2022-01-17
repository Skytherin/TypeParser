namespace TypeParser.Matchers
{
    internal class StringMatcher : RxMatcher
    {
        public StringMatcher() : base(@"[a-zA-Z]+", s => s)
        {
        }
    }
}