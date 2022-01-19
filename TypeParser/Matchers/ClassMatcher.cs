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
                    .Select(property => new InfoType(property.PropertyType, property.GetCustomAttributes().ToList(),
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
                        return new InfoType(property.ParameterType, property.GetCustomAttributes().ToList());
                    })
                    .ToList();
            }
        }

        private record InfoType(Type Type, IReadOnlyList<Attribute> Attributes, 
            PropertyInfo? PropertyInfo = null);

        private IReadOnlyList<ITypeMatcher> Compile()
        {
            if (Matchers == null)
            {
                Matchers = Properties.Select(it =>
                    Compiler.Compile(it.Type,
                        it.Attributes.OfType<FormatAttribute>().FirstOrDefault()?.Format())).ToList();
            }

            return Matchers;
        }

        public ITypeMatcher.Result? Match(string input)
        {
            var actuals = new List<object?>();

            var alternativeGroups = SplitIntoAlternativeGroups();

            foreach (var alternativeGroup in alternativeGroups)
            {
                var result = MatchAlternates(alternativeGroup, input);
                if (result is not {} r) return null;
                actuals.AddRange(r.Actuals);
                input = r.Remainder;
            }

            var instance = Instantiate(actuals);
            return new(instance, input);
        }

        private IReadOnlyList<IReadOnlyList<ITypeMatcher>> SplitIntoAlternativeGroups()
        {
            var alternativeGroups = new List<List<ITypeMatcher>>();
            List<ITypeMatcher>? g = null;
            foreach (var (property, propertyMatcher) in Properties.Zip(Compile()))
            {
                var rxAlternate = property.Attributes.OfType<RxAlternate>().FirstOrDefault();
                if (rxAlternate == null)
                {
                    alternativeGroups.Add(new() { propertyMatcher });
                }
                else if (rxAlternate.Restart || g == null)
                {
                    g = new() { propertyMatcher };
                    alternativeGroups.Add(g);
                }
                else
                {
                    g.Add(propertyMatcher);
                }
            }

            return alternativeGroups;
        }

        private static (IReadOnlyList<object?> Actuals, string Remainder)? MatchAlternates(IReadOnlyList<ITypeMatcher> alternatives, string input)
        {
            var found = false;
            var actuals = new List<object?>();

            foreach (var propertyMatcher in alternatives)
            {
                input = input.TrimStart();
                if (found)
                {
                    actuals.Add(null);
                    continue;
                }

                var matched = propertyMatcher.Match(input.TrimStart());

                if (matched == null)
                {
                    actuals.Add(null);
                    continue;
                }

                found = true;
                actuals.Add(matched.Object);
                input = matched.Remainder;
            }

            if (!found)
            {
                return null;
            }

            return (actuals, input);
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