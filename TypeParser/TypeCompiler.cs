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
    internal class TypeCompiler
    {
        private readonly Dictionary<Type, ITypeMatcher> CompiledTypes = new();

        public ITypeMatcher TypeParserForType(Type type, Format? format = null)
        {
            format ??= FormatExtensions.DefaultFormat();
            if (type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                type.GenericTypeArguments.Length == 1 &&
                type.GenericTypeArguments.FirstOrDefault() is { } targ &&
                type.IsAssignableTo(typeof(Nullable<>).MakeGenericType(targ)))
            {
                return new OptionalMatcher(TypeParserForType(targ, format with {Optional = false}));
            }

            if (format is {Optional: true})
            {
                return new OptionalMatcher(TypeParserForType(type, format with {Optional = false}));
            }

            if (format.Before is not null)
            {
                return new BeforeMatcher(format.Before, TypeParserForType(type, format with {Before = null}));
            }

            if (format.After is not null)
            {
                return new AfterMatcher(format.After, TypeParserForType(type, format with {After = null}));
            }

            if (type == typeof(int)) return new IntMatcher(format.Regex);
            if (type == typeof(long)) return new LongMatcher(format.Regex);
            if (type == typeof(char)) return new CharMatcher(format.Regex);
            if (type == typeof(string)) return new StringMatcher(format.Regex);

            if (CompiledTypes.TryGetValue(type, out var compiledType)) return compiledType;

            if (type.IsGenericType &&
                type.GenericTypeArguments.Length == 1 &&
                type.GenericTypeArguments.FirstOrDefault() is { } targ2 &&
                typeof(List<>).MakeGenericType(targ2) is {} listType &&
                type.IsAssignableFrom(listType))
            {
                var elementMatcher = TypeParserForType(targ2);
                var listMatcherType = typeof(ListMatcher<>).MakeGenericType(targ2);
                return (ITypeMatcher)Activator.CreateInstance(listMatcherType, elementMatcher, format)!;
            }

            if (type.IsEnum)
            {
                var mi = typeof(TypeCompiler).GetMethod("GetEnumMap", BindingFlags.NonPublic | BindingFlags.Static);
                var fooRef = mi!.MakeGenericMethod(type);
                var map = (Dictionary<string, object>)fooRef.Invoke(null, null)!;

                var alternation = map.Keys.Select(it => $"({it})").Join("|");

                return new RxMatcher<object?>(new(alternation), s => map[s.ToLower()]);
            }

            if (type.IsGenericType &&
                type.IsAssignableTo(typeof(ITuple)) || type.IsClass)
            {
                var classMatcher = new ClassMatcher(type, this);
                CompiledTypes.Add(type, classMatcher);
                return classMatcher;
            }

            throw new ApplicationException();
        }

        [UsedImplicitly]
#pragma warning disable IDE0051
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