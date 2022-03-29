using System;
using System.Collections.Generic;
using System.Linq;
using Common.Utils;
using JetBrains.Annotations;
using TypeParser.Matchers;

namespace TypeParser
{
    public interface ITypeParser<T>
    {
        record Result(T Value, string Remainder);

        Result? Match(string input);
    }

    internal class TypeParserFacade<T>: ITypeParser<T>
    {
        private readonly ITypeMatcher Actual;

        public TypeParserFacade(ITypeMatcher actual)
        {
            Actual = actual;
        }

        public ITypeParser<T>.Result? Match(string input)
        {
            var m = Actual.Match(input);
            if (m == null) return null;
            return new((T)m.Value!, m.Remainder);
        }
    }

    [UsedImplicitly]
    public static class TypeCompiler
    {
        [UsedImplicitly]
        public static ITypeParser<T> Compile<T>(Format? format = null)
        {
            var compiler = new InternalTypeCompiler();
            return new TypeParserFacade<T>(new EntireStringMatcher(compiler.Compile(typeof(T), format?.Format())));
        }

        public static ITypeParser<T> GetTypeParser<T>(Format? format = null)
        {
            var compiler = new InternalTypeCompiler();
            return new TypeParserFacade<T>(compiler.Compile(typeof(T), format?.Format()));
        }

        public static T Parse<T>(string input, Format? format = null)
        {
            var compiled = Compile<T>(format);
            var m = compiled.Match(input);
            if (m != null)
            {
                return m.Value;
            }

            throw new ApplicationException("Failed to match.");
        }

        [UsedImplicitly]
        public static List<T> ParseLines<T>(string input, Format? format = null)
        {
            var compiled = Compile<T>(format);
            return input.Lines().Select(line =>
            {
                var m = compiled.Match(line);
                if (m != null) return m.Value;
                throw new ApplicationException($"Matching error on line {line}");
            }).ToList();
        }

        [UsedImplicitly]
        public static T? ParseOrDefault<T>(string input)
        {
            try
            {
                return Parse<T>(input);
            }
            catch
            {
                return default;
            }
        }
    }
}