using TypeParser.UtilityClasses;

namespace TypeParser.Matchers
{
    internal class AlternativeMatcher<T1, T2> : ITypeMatcher
    {
        private readonly ITypeMatcher T1Matcher;
        private readonly ITypeMatcher T2Matcher;

        public AlternativeMatcher(TypeCompiler typeCompiler)
        {
            T1Matcher = typeCompiler.Compile(typeof(T1));
            T2Matcher = typeCompiler.Compile(typeof(T2));
        }

        public ITypeMatcher.Result? Match(string input)
        {
            var m = T1Matcher.Match(input);
            if (m != null)
            {
                return new(new FirstAlternative<T1, T2>((T1)m.Object!), m.Remainder);
            }
            m = T2Matcher.Match(input);
            if (m != null)
            {
                return new(new SecondAlternative<T1, T2>((T2)m.Object!), m.Remainder);
            }

            return null;
        }
    }
}