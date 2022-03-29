using System;
using System.Collections.Generic;
using System.Linq;

namespace TypeParser.UtilityClasses
{
    public interface IAlternative<out T1, out T2>
    {
        TResult Select<TResult>(Func<T1, TResult> whenFirst, Func<T2, TResult> whenSecond);
    }

    public static class Alternative2Extensions
    {
        public static object? AsObject<T1, T2>(this IAlternative<T1, T2> self) => self.Select(it => it as object, it => it);
    }

    internal class FirstAlternative<T1, T2> : IAlternative<T1, T2>
    {
        private readonly T1 Value;

        public FirstAlternative(T1 value)
        {
            Value = value;
        }

        public TResult Select<TResult>(Func<T1, TResult> whenFirst, Func<T2, TResult> whenSecond)
        {
            return whenFirst(Value);
        }

        public override string ToString() => Value?.ToString() ?? "null";
    }

    internal class SecondAlternative<T1, T2> : IAlternative<T1, T2>
    {
        private readonly T2 Value;

        public SecondAlternative(T2 value)
        {
            Value = value;
        }

        public TResult Select<TResult>(Func<T1, TResult> whenFirst, Func<T2, TResult> whenSecond)
        {
            return whenSecond(Value);
        }

        public override string ToString() => Value?.ToString() ?? "null";
    }
}