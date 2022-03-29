using TypeParser.UtilityClasses;

namespace TypeParser.Matchers
{
    internal class AlternativeMatcher<T1, T2> : ITypeMatcher
    {
        private readonly ITypeMatcher T1Matcher;
        private readonly ITypeMatcher T2Matcher;

        public AlternativeMatcher(InternalTypeCompiler typeCompiler)
        {
            T1Matcher = typeCompiler.Compile(typeof(T1));
            T2Matcher = typeCompiler.Compile(typeof(T2));
        }

        public ITypeMatcher.Result? Match(string input)
        {
            var m = T1Matcher.Match(input);
            if (m != null)
            {
                return new(new FirstAlternative<T1, T2>((T1)m.Value!), m.Remainder);
            }
            m = T2Matcher.Match(input);
            if (m != null)
            {
                return new(new SecondAlternative<T1, T2>((T2)m.Value!), m.Remainder);
            }

            return null;
        }
    }

    internal class AlternativeMatcher<T1, T2, T3> : ITypeMatcher
    {
        private readonly ITypeMatcher T1Matcher;
        private readonly ITypeMatcher T2Matcher;
        private readonly ITypeMatcher T3Matcher;

        public AlternativeMatcher(InternalTypeCompiler typeCompiler)
        {
            T1Matcher = typeCompiler.Compile(typeof(T1));
            T2Matcher = typeCompiler.Compile(typeof(T2));
            T3Matcher = typeCompiler.Compile(typeof(T3));
        }

        public ITypeMatcher.Result? Match(string input)
        {
            var m = T1Matcher.Match(input);
            if (m != null)
            {
                return new(new FirstAlternative<T1, T2, T3>((T1)m.Value!), m.Remainder);
            }
            m = T2Matcher.Match(input);
            if (m != null)
            {
                return new(new SecondAlternative<T1, T2, T3>((T2)m.Value!), m.Remainder);
            }
            m = T3Matcher.Match(input);
            if (m != null)
            {
                return new(new ThirdAlternative<T1, T2, T3>((T3)m.Value!), m.Remainder);
            }

            return null;
        }
    }

    internal class AlternativeMatcher<T1, T2, T3, T4, T5> : ITypeMatcher
    {
        private readonly ITypeMatcher T1Matcher;
        private readonly ITypeMatcher T2Matcher;
        private readonly ITypeMatcher T3Matcher;
        private readonly ITypeMatcher T4Matcher;
        private readonly ITypeMatcher T5Matcher;

        public AlternativeMatcher(InternalTypeCompiler typeCompiler)
        {
            T1Matcher = typeCompiler.Compile(typeof(T1));
            T2Matcher = typeCompiler.Compile(typeof(T2));
            T3Matcher = typeCompiler.Compile(typeof(T3));
            T4Matcher = typeCompiler.Compile(typeof(T4));
            T5Matcher = typeCompiler.Compile(typeof(T5));
        }

        public ITypeMatcher.Result? Match(string input)
        {
            var m = T1Matcher.Match(input);
            if (m != null)
            {
                return new(new FirstAlternative<T1, T2, T3, T4, T5>((T1)m.Value!), m.Remainder);
            }
            m = T2Matcher.Match(input);
            if (m != null)
            {
                return new(new SecondAlternative<T1, T2, T3, T4, T5>((T2)m.Value!), m.Remainder);
            }
            m = T3Matcher.Match(input);
            if (m != null)
            {
                return new(new ThirdAlternative<T1, T2, T3, T4, T5>((T3)m.Value!), m.Remainder);
            }
            m = T4Matcher.Match(input);
            if (m != null)
            {
                return new(new FourthAlternative<T1, T2, T3, T4, T5>((T4)m.Value!), m.Remainder);
            }
            m = T5Matcher.Match(input);
            if (m != null)
            {
                return new(new FifthAlternative<T1, T2, T3, T4, T5>((T5)m.Value!), m.Remainder);
            }

            return null;
        }
    }
}