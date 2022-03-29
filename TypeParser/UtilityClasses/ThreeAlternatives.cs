using System;

namespace TypeParser.UtilityClasses
{
    public interface IAlternative<out T1, out T2, out T3>
    {
        TResult Select<TResult>(Func<T1, TResult> whenFirst, Func<T2, TResult> whenSecond, Func<T3, TResult> whenThird);
    }

    internal class FirstAlternative<T1, T2, T3> : IAlternative<T1, T2, T3>
    {
        private readonly T1 Value;

        public FirstAlternative(T1 value)
        {
            Value = value;
        }

        public TResult Select<TResult>(Func<T1, TResult> whenFirst, Func<T2, TResult> whenSecond, Func<T3, TResult> whenThird)
        {
            return whenFirst(Value);
        }

        public override string ToString() => Value?.ToString() ?? "null";
    }

    internal class SecondAlternative<T1, T2, T3> : IAlternative<T1, T2, T3>
    {
        private readonly T2 Value;

        public SecondAlternative(T2 value)
        {
            Value = value;
        }

        public TResult Select<TResult>(Func<T1, TResult> whenFirst, Func<T2, TResult> whenSecond, Func<T3, TResult> whenThird)
        {
            return whenSecond(Value);
        }

        public override string ToString() => Value?.ToString() ?? "null";
    }

    internal class ThirdAlternative<T1, T2, T3> : IAlternative<T1, T2, T3>
    {
        private readonly T3 Value;

        public ThirdAlternative(T3 value)
        {
            Value = value;
        }

        public TResult Select<TResult>(Func<T1, TResult> whenFirst, Func<T2, TResult> whenSecond, Func<T3, TResult> whenThird)
        {
            return whenThird(Value);
        }
    }

}