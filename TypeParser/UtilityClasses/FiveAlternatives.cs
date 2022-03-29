using System;

namespace TypeParser.UtilityClasses
{
    public interface IAlternative<out T1, out T2, out T3, out T4, out T5>
    {
        TResult Select<TResult>(Func<T1, TResult> whenFirst, Func<T2, TResult> whenSecond, Func<T3, TResult> whenThird,
            Func<T4, TResult> whenFourth, Func<T5, TResult> whenFifth);
    }

    internal class FirstAlternative<T1, T2, T3, T4, T5> : IAlternative<T1, T2, T3, T4, T5>
    {
        private readonly T1 Value;

        public FirstAlternative(T1 value)
        {
            Value = value;
        }

        public TResult Select<TResult>(Func<T1, TResult> whenFirst, Func<T2, TResult> whenSecond, Func<T3, TResult> whenThird, Func<T4, TResult> whenFourth, Func<T5, TResult> whenFifth)
        {
            return whenFirst(Value);
        }

        public override string ToString() => Value?.ToString() ?? "null";
    }

    internal class SecondAlternative<T1, T2, T3, T4, T5> : IAlternative<T1, T2, T3, T4, T5>
    {
        private readonly T2 Value;

        public SecondAlternative(T2 value)
        {
            Value = value;
        }

        public TResult Select<TResult>(Func<T1, TResult> whenFirst, Func<T2, TResult> whenSecond, Func<T3, TResult> whenThird, Func<T4, TResult> whenFourth, Func<T5, TResult> whenFifth)
        {
            return whenSecond(Value);
        }

        public override string ToString() => Value?.ToString() ?? "null";
    }

    internal class ThirdAlternative<T1, T2, T3, T4, T5> : IAlternative<T1, T2, T3, T4, T5>
    {
        private readonly T3 Value;

        public ThirdAlternative(T3 value)
        {
            Value = value;
        }

        public TResult Select<TResult>(Func<T1, TResult> whenFirst, Func<T2, TResult> whenSecond, Func<T3, TResult> whenThird, Func<T4, TResult> whenFourth, Func<T5, TResult> whenFifth)
        {
            return whenThird(Value);
        }

        public override string ToString() => Value?.ToString() ?? "null";
    }

    internal class FourthAlternative<T1, T2, T3, T4, T5> : IAlternative<T1, T2, T3, T4, T5>
    {
        private readonly T4 Value;

        public FourthAlternative(T4 value)
        {
            Value = value;
        }

        public TResult Select<TResult>(Func<T1, TResult> whenFirst, Func<T2, TResult> whenSecond, Func<T3, TResult> whenThird, Func<T4, TResult> whenFourth, Func<T5, TResult> whenFifth)
        {
            return whenFourth(Value);
        }

        public override string ToString() => Value?.ToString() ?? "null";
    }

    internal class FifthAlternative<T1, T2, T3, T4, T5> : IAlternative<T1, T2, T3, T4, T5>
    {
        private readonly T5 Value;

        public FifthAlternative(T5 value)
        {
            Value = value;
        }

        public TResult Select<TResult>(Func<T1, TResult> whenFirst, Func<T2, TResult> whenSecond, Func<T3, TResult> whenThird, Func<T4, TResult> whenFourth, Func<T5, TResult> whenFifth)
        {
            return whenFifth(Value);
        }

        public override string ToString() => Value?.ToString() ?? "null";
    }
}