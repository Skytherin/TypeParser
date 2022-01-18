namespace TypeParser.Matchers
{
    internal interface ITypeMatcher
    {
        record Result(object? Object, string Remainder);
        Result? Match(string input);
    }
}