namespace TypeParser.Matchers
{
    internal class CharMatcher : RxMatcher
    {
        public CharMatcher() : base(@"\S", s => s[0])
        {
        }
    }
}