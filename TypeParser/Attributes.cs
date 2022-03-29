using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Common.Utils;
using JetBrains.Annotations;

namespace TypeParser
{
    internal record InternalFormat(Regex? Before, Regex? After, bool Optional, Regex? Regex, int Min, int Max, Regex? Separator);

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class Format : Attribute
    {
        public string? After { get; init; }

        public string? Before { get; init; }

        public bool Optional { get; init; }

        [RegexPattern]
        public string? Regex { get; init; }

        public string? Choices { get; init; }

        public int Min { get; init; }
        public int Max { get; init; } = int.MaxValue;
        public string? Separator { get; init; }
    }

    internal static class FormatExtensions
    {
        public static InternalFormat Format(this Format format) => new(Convert(format.Before), 
            Convert(format.After), 
            format.Optional, 
            ConvertChoices(format.Choices) ?? Convert2(format.Regex), 
            format.Min, 
            format.Max, 
            Convert(format.Separator));

        private static Regex? Convert2(string? rx)
        {
            if (rx == null) return null;
            if (!rx.StartsWith("^")) return new($"^({rx})");
            return new(rx);
        }

        private static Regex? ConvertChoices(string? formatChoices)
        {
            if (formatChoices == null) return null;

            var choices = formatChoices.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            foreach (var (earlyChoice, index) in choices.WithIndices())
            {
                foreach (var laterChoice in choices.Skip(index + 1))
                {
                    if (laterChoice.StartsWith(earlyChoice, true, CultureInfo.InvariantCulture))
                    {
                        throw new ApplicationException($"Choice {laterChoice} will never be matched because choice {earlyChoice} will be matched first. To remove this error, rearrange choices.");
                    }
                }
            }

            var rx = choices
                .Select(it => $"({Regex.Escape(it)})").Join("|");
            return new Regex($"^({rx})", RegexOptions.IgnoreCase);
        }

        public static InternalFormat DefaultFormat() => new(null, null, false, null, 0, int.MaxValue, null);

        private static Regex? Convert(string? s)
        {
            if (s == null) return null;
            if (s.StartsWith("/") && s.EndsWith("/") && s.Length > 2)
            {
                s = s[1..^1];
                if (s.StartsWith('^')) s = s[1..];
                return new Regex(@$"^\s*({s})");
            }

            return new($"^({Regex.Escape(s)})");
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class Alternate : Attribute
    {
        public bool Restart { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class Ignore : Attribute
    {
    }
}