using System;
using System.Collections.Generic;
using System.Linq;

namespace TypeParser.UtilityClasses
{
    public interface IAlternative<out T1, out T2>
    {
        T3 Select<T3>(Func<T1, T3> whenFirst, Func<T2, T3> whenSecond);
    }

    internal class FirstAlternative<T1, T2> : IAlternative<T1, T2>
    {
        private readonly T1 Value;

        public FirstAlternative(T1 value)
        {
            Value = value;
        }

        public T3 Select<T3>(Func<T1, T3> whenFirst, Func<T2, T3> whenSecond)
        {
            return whenFirst(Value);
        }
    }

    internal class SecondAlternative<T1, T2> : IAlternative<T1, T2>
    {
        private readonly T2 Value;

        public SecondAlternative(T2 value)
        {
            Value = value;
        }

        public T3 Select<T3>(Func<T1, T3> whenFirst, Func<T2, T3> whenSecond)
        {
            return whenSecond(Value);
        }
    }
}