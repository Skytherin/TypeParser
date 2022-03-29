using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Common.Utils;
using JetBrains.Annotations;
using TypeParser.Matchers;
using TypeParser.UtilityClasses;

namespace TypeParser
{
    internal class InternalTypeCompiler
    {
        private readonly Dictionary<Type, ITypeMatcher> CompiledTypes = new();

        internal ITypeMatcher Compile(Type type, InternalFormat? format = null)
        {
            format ??= FormatExtensions.DefaultFormat();
            if (type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                type.GenericTypeArguments.Length == 1 &&
                type.GenericTypeArguments.FirstOrDefault() is { } targ &&
                type.IsAssignableTo(typeof(Nullable<>).MakeGenericType(targ)))
            {
                return new OptionalMatcher(Compile(targ, format with {Optional = false}));
            }

            if (format is {Optional: true})
            {
                return new OptionalMatcher(Compile(type, format with {Optional = false}));
            }

            if (format.Before is not null)
            {
                return new BeforeMatcher(format.Before, Compile(type, format with {Before = null}));
            }

            if (format.After is not null)
            {
                return new AfterMatcher(format.After, Compile(type, format with {After = null}));
            }

            if (type == typeof(int)) return new IntMatcher(format.Regex);
            if (type == typeof(long)) return new LongMatcher(format.Regex);
            if (type == typeof(char)) return new CharMatcher(format.Regex);
            if (type == typeof(string)) return new StringMatcher(format.Regex);

            if (CompiledTypes.TryGetValue(type, out var compiledType)) return compiledType;

            if (type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(IAlternative<,>))
            {
                var atype = typeof(AlternativeMatcher<,>).MakeGenericType(type.GenericTypeArguments);
                var matcher = (ITypeMatcher)Activator.CreateInstance(atype, this)!;
                CompiledTypes.Add(type, matcher);
                return matcher;
            }

            if (type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(IAlternative<,,>))
            {
                var atype = typeof(AlternativeMatcher<,,>).MakeGenericType(type.GenericTypeArguments);
                var matcher = (ITypeMatcher)Activator.CreateInstance(atype, this)!;
                CompiledTypes.Add(type, matcher);
                return matcher;
            }

            if (type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(IAlternative<,,,,>))
            {
                var atype = typeof(AlternativeMatcher<,,,,>).MakeGenericType(type.GenericTypeArguments);
                var matcher = (ITypeMatcher)Activator.CreateInstance(atype, this)!;
                CompiledTypes.Add(type, matcher);
                return matcher;
            }

            if (type.IsGenericType &&
                type.GenericTypeArguments.Length == 2 &&
                type.GenericTypeArguments.FirstOrDefault() is { } darg1 &&
                type.GenericTypeArguments.Skip(1).FirstOrDefault() is { } darg2 &&
                typeof(Dictionary<,>).MakeGenericType(darg1, darg2) is { } dtype &&
                type.IsAssignableFrom(dtype))
            {
                var keyMatcher = Compile(darg1);
                var valueMatcher = Compile(darg2);
                var dMatcher = typeof(DictionaryMatcher<,>).MakeGenericType(darg1, darg2);
                return (ITypeMatcher)Activator.CreateInstance(dMatcher, keyMatcher, valueMatcher, format)!;
            }

            if (type.IsGenericType &&
                type.GenericTypeArguments.Length == 1 &&
                type.GenericTypeArguments.FirstOrDefault() is { } targ2 &&
                typeof(List<>).MakeGenericType(targ2) is {} listType &&
                type.IsAssignableFrom(listType))
            {
                var elementMatcher = Compile(targ2);
                var listMatcherType = typeof(ListMatcher<>).MakeGenericType(targ2);
                return (ITypeMatcher)Activator.CreateInstance(listMatcherType, elementMatcher, format)!;
            }

            if (type.IsEnum)
            {
                var mi = typeof(InternalTypeCompiler).GetMethod("GetEnumMap", BindingFlags.NonPublic | BindingFlags.Static);
                var fooRef = mi!.MakeGenericMethod(type);
                var map = (Dictionary<string, int>)fooRef.Invoke(null, null)!;

                var alternation = map.Keys.Select(it => $"({it})").Join("|");

                return new TypedRxMatcher<object?>(new(alternation), s => map[s.ToLower()]);
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