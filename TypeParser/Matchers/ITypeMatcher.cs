namespace TypeParser.Matchers
{
    public interface ITypeMatcher<T>
    {
        bool TryScan(string input, out T output, out string remainder);
    }
}