using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.Utils;

namespace TypeParser.Matchers
{
    internal class ClassMatcher : ITypeMatcher
    {
        private readonly Type MyType;

        public ClassMatcher(Type type)
        {
            MyType = type;
        }

        private record InfoType(string Name, Type Type, IReadOnlyList<Attribute> Attributes, PropertyInfo? PropertyInfo = null);

        private IReadOnlyList<InfoType> FindPropertiesForClass()
        {
            var ctors = MyType.GetConstructors();
            if (ctors.Any(ctor => ctor.GetParameters().Length == 0))
            {
                return MyType.GetProperties()
                    .Where(p => p.SetMethod != null)
                    .Where(p => p.GetCustomAttribute<RxIgnore>() == null)
                    .Select(it => new InfoType(it.Name, it.PropertyType, it.GetCustomAttributes().ToList(), it))
                    .ToList();
            }

            return ctors.First().GetParameters()
                .WithIndices()
                .Select(p => new InfoType(p.Value.Name ?? $"p{p.Index}", p.Value.ParameterType, p.Value.GetCustomAttributes().ToList()))
                .ToList();
        }

        public bool TryScan(string input, out object? output, out string remainder)
        {
            var properties = FindPropertiesForClass();

            var alternations = new List<string>();

            var actuals = new List<object?>();

            var keepAlternate = false;

            var first = true;
            remainder = input;
            foreach (var property in properties)
            {
                if (!first)
                {
                    input = input.TrimStart();
                }

                first = false;

                var rxFormat = property.Attributes.OfType<RxFormat>().FirstOrDefault() ?? new RxFormat();
                var rxRepeat = property.Attributes.OfType<RxRepeat>().FirstOrDefault() ?? new RxRepeat();
                var matcher = TypeMatcherHelper.TypeParserForType(property.Type, rxFormat, rxRepeat);
                var rxAlternate = property.Attributes.OfType<RxAlternate>().FirstOrDefault();

                if (keepAlternate)
                {
                    if (rxAlternate == null || rxAlternate.Restart)
                    {
                        throw new ApplicationException("Never matched an alternate.");
                    }
                }

                if (rxAlternate != null && !keepAlternate)
                {
                    continue;
                }

                var matched = matcher.TryScan(input, out var needle, out remainder);

                if (!matched && rxFormat.Optional)
                {
                    actuals.Add(null);
                    continue;
                }

                if (!matched && rxAlternate is { })
                {
                    actuals.Add(null);
                    keepAlternate = true;
                    continue;
                }

                if (!matched)
                {
                    output = null;
                    return false;
                }

                keepAlternate = false;

                actuals.Add(needle);
                input = remainder;
            }

            
            var ctors = MyType.GetConstructors();

            if (ctors.Any(ctor => ctor.GetParameters().Length == 0))
            {
                var instance = Activator.CreateInstance(MyType)!;
                foreach (var (actual, property) in actuals.Zip(properties))
                {
                    property.PropertyInfo!.SetValue(instance, actual);
                }

                remainder = input;
                output = instance;
                return true;
            }

            output = ctors.First().Invoke(actuals.ToArray());
            remainder = input;
            return true;
        }
    }
}