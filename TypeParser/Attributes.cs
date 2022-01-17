using System;

namespace TypeParser
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class RxFormat : Attribute
    {
        public RxFormat()
        {

        }

        public RxFormat(string? before, string? after, bool optional)
        {
            Before = before;
            After = after;
            Optional = optional;
        }

        public string? After { get; init; }

        public string? Before { get; init; }

        public bool Optional { get; init; }
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

    [AttributeUsage(AttributeTargets.All)]
    public class RxRepeat : Attribute
    {
        public const string DefaultSeparator = "\\s+";
        public int Min { get; init; }
        public int Max { get; init; } = int.MaxValue;
        public string Separator { get; init; } = DefaultSeparator;
    }
}