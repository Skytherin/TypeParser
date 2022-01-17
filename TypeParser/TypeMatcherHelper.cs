using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Common.Utils;
using JetBrains.Annotations;
using TypeParser.Matchers;

namespace TypeParser
{
    internal static class TypeMatcherHelper
    {
        public static ITypeMatcher TypeParserForType(Type type, RxFormat? format = null, RxRepeat? repeat = null)
        {
            if (type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                type.GenericTypeArguments.Length == 1 &&
                type.GenericTypeArguments.FirstOrDefault() is { } targ &&
                type.IsAssignableTo(typeof(Nullable<>).MakeGenericType(targ)))
            {
                return new OptionalMatcher(TypeParserForType(targ, new RxFormat(format?.Before, format?.After, false), repeat));
            }

            if (format is {Optional: true})
            {
                return new OptionalMatcher(TypeParserForType(type, new RxFormat(format.Before, format.After, false), repeat));
            }

            if (format?.Before is not null)
            {
                return new BeforeMatcher(format.Before, TypeParserForType(type, new RxFormat(null, format.After, format.Optional), repeat));
            }

            if (format?.After is not null)
            {
                return new AfterMatcher(format.After, TypeParserForType(type, new RxFormat(format.Before, null, format.Optional), repeat));
            }

            if (type == typeof(int)) return new IntMatcher();
            if (type == typeof(long)) return new LongMatcher();
            if (type == typeof(char)) return new CharMatcher();
            if (type == typeof(string)) return new StringMatcher();
            

            if (type.IsGenericType &&
                type.GenericTypeArguments.Length == 1 &&
                type.GenericTypeArguments.FirstOrDefault() is { } targ2 &&
                type.IsAssignableFrom(typeof(List<>).MakeGenericType(targ2)))
            {
                var lmType = typeof(ListMatcher<>).MakeGenericType(targ2);
                var instance = Activator.CreateInstance(lmType, TypeParserForType(targ2), repeat);
                return (ITypeMatcher)instance!;
            }

            if (type.IsEnum)
            {
                var mi = typeof(TypeMatcherHelper).GetMethod("GetEnumMap", BindingFlags.NonPublic | BindingFlags.Static);
                var fooRef = mi!.MakeGenericMethod(type);
                var map = (Dictionary<string, int>)fooRef.Invoke(null, null)!;

                var alternation = map.Keys.Select(it => $"({it})").Join("|");

                return new RxMatcher(alternation, s => map[s.ToLower()]);
            }

            if (type.IsGenericType &&
                type.IsAssignableTo(typeof(ITuple)))
            {
                return new ClassMatcher(type);
            }

            if (type.IsClass)
            {
                return new ClassMatcher(type);
            }

            throw new ApplicationException();
        }

        [UsedImplicitly]
        private static Dictionary<string, int> GetEnumMap<T>()
        {
            var enumValues = typeof(T).GetEnumValues();
            var result = new Dictionary<string, int>();

            foreach (T value in enumValues)
            {
                var memberInfo = typeof(T)
                    .GetMember(value.ToString()!)
                    .First();

                if (memberInfo.GetCustomAttribute<DescriptionAttribute>() is { } description)
                {
                    result[description.Description.ToLower()] = Convert.ToInt32(value);
                }
                else
                {
                    result[memberInfo.Name.ToLower()] = Convert.ToInt32(value);
                }
            }

            return result;
        }
    }
}