using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Common.Utils;

namespace TypeParser.Matchers
{
    internal class ClassMatcher : ITypeMatcher
    {
        private readonly TypeCompiler Compiler;
        private readonly ConstructorInfo Ctor;
        private readonly IReadOnlyList<InfoType> Properties;
        private IReadOnlyList<ITypeMatcher>? Matchers;

        public ClassMatcher(Type type, TypeCompiler compiler)
        {
            Compiler = compiler;
            var ctors = type.GetConstructors();
            Ctor = ctors.FirstOrDefault(ctor => ctor.GetParameters().Length == 0) ?? ctors.First();

            if (Ctor.GetParameters().Length == 0)
            {
                Properties = type.GetProperties()
                    .Where(p => p.SetMethod != null)
                    .Where(p => p.GetCustomAttribute<RxIgnore>() == null)
                    .Select(property => new InfoType(property.Name, property.PropertyType, property.GetCustomAttributes().ToList(),
                        property))
                    .ToList();
            }
            else
            {
                Properties = ctors.First().GetParameters()
                    .WithIndices()
                    .Select(p =>
                    {
                        var property = p.Value;
                        return new InfoType(property.Name ?? $"p{p.Index}", property.ParameterType, property.GetCustomAttributes().ToList());
                    })
                    .ToList();
            }
        }

        private record InfoType(string Name, Type Type, IReadOnlyList<Attribute> Attributes, 
            PropertyInfo? PropertyInfo = null);

        private IReadOnlyList<ITypeMatcher> Compile()
        {
            if (Matchers == null)
            {
                Matchers = Properties.Select(it =>
                    Compiler.TypeParserForType(it.Type,
                        it.Attributes.OfType<FormatAttribute>().FirstOrDefault()?.Format())).ToList();
            }

            return Matchers;
        }

        public ITypeMatcher.Result? Match(string input)
        {
            var actuals = new List<object?>();

            var alternateFound = false;
            var previousWasAlternate = false;

            foreach (var (property, propertyMatcher) in Properties.Zip(Compile()))
            {
                var rxAlternate = property.Attributes.OfType<RxAlternate>().FirstOrDefault();

                if (previousWasAlternate && (rxAlternate == null || rxAlternate.Restart))
                {
                    if (!alternateFound) return null;
                    previousWasAlternate = false;
                    alternateFound = false;
                }

                if (rxAlternate != null && previousWasAlternate && alternateFound)
                {
                    actuals.Add(null);
                    continue;
                }

                if (rxAlternate is { })
                {
                    previousWasAlternate = true;
                }

                var matched = propertyMatcher.Match(input.TrimStart());

                if (matched == null)
                {
                    var rxFormat = property.Attributes.OfType<FormatAttribute>().FirstOrDefault() ?? new FormatAttribute();
                    if (rxFormat.Optional || rxAlternate is {})
                    {
                        actuals.Add(null);
                        continue;
                    }

                    return null;
                }

                if (rxAlternate is { })
                {
                    alternateFound = true;
                }

                actuals.Add(matched.Object);
                input = matched.Remainder;
            }

            if (previousWasAlternate && !alternateFound) return null;
            var instance = Instantiate(actuals);
            Debug.WriteLine($"Matched {instance.GetType().Name}; tail = {input}");
            return new(instance, input);
        }

        private object Instantiate(IEnumerable<object?> actuals)
        {
            if (Ctor.GetParameters().Length == 0)
            {
                var instance = Ctor.Invoke(Array.Empty<object?>());
                foreach (var (actual, property) in actuals.Zip(Properties))
                {
                    property.PropertyInfo!.SetValue(instance, actual);
                }

                return instance;
            }

            return Ctor.Invoke(actuals.ToArray());
        }
    }
}