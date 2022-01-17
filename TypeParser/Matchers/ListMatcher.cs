using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TypeParser.Matchers
{
    internal class ListMatcher<T> : ITypeMatcher
    {
        private readonly ITypeMatcher ElementMatcher;
        private readonly RxRepeat Repeat;

        public ListMatcher(ITypeMatcher elementMatcher, RxRepeat? repeat)
        {
            Repeat = repeat ?? new RxRepeat();
            ElementMatcher = elementMatcher;
        }

        public bool TryScan(string input, out object? output, out string remainder)
        {
            var instance = new List<T>();

            var first = true;
            remainder = input;
            output = null;
            while (instance.Count < Repeat.Max)
            {
                if (!first)
                {
                    var m1 = Regex.Match(input, @$"^\s*{Repeat.Separator}\s*");
                    if (!m1.Success) break;
                    input = input[m1.Length..];
                }

                first = false;

                var m = ElementMatcher.TryScan(input, out var element, out var remainder2);
                if (!m)
                {
                    return false;
                }

                if (element is T { } e) instance.Add(e);
                else throw new ApplicationException("Mismatched element in list. This should never happen.");

                input = remainder2;
            }

            if (instance.Count < Repeat.Min)
            {
                return false;
            }

            output = instance;
            remainder = input;
            return true;
        }
    }
}