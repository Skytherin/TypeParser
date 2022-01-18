using System;
using System.Text.RegularExpressions;

namespace TypeParser
{
    internal record Format(Regex? Before, Regex? After, bool Optional, Regex? Regex, int Min, int Max, Regex Separator);

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class FormatAttribute : Attribute
    {
        public string? After { get; init; }

        public string? Before { get; init; }

        public bool Optional { get; init; }

        public string? Regex { get; init; }

        public int Min { get; init; }
        public int Max { get; init; } = int.MaxValue;
        public string? Separator { get; init; }
    }

    internal static class FormatExtensions
    {
        internal static Regex DefaultSeparator = new(@"\s+");
        public static Format Format(this FormatAttribute format) => new(Convert(format.Before), 
            Convert(format.After), 
            format.Optional, 
            Convert(format.Regex), 
            format.Min, 
            format.Max, 
            Convert(format.Separator) ?? DefaultSeparator);

        public static Format DefaultFormat() => new(null, null, false, null, 0, Int32.MaxValue, DefaultSeparator);

        private static Regex? Convert(string? s)
        {
            if (s == null) return null;
            if (s.StartsWith("/") && s.EndsWith("/") && s.Length > 2)
            {
                s = s[1..^1];
                if (s.StartsWith('^')) s = s[1..];
                return new Regex(@$"^\s*({s})");
            }

            return new(Regex.Escape(s));
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class RxAlternate : Attribute
    {
        public bool Restart { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class RxIgnore : Attribute
    {
    }
}