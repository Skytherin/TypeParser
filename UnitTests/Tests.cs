using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Common.Utils;
using JetBrains.Annotations;
using TypeParser;
using TypeParser.UtilityClasses;


namespace UnitTests
{
    [TestFixture]
    public class Tests
    {
        private readonly Random Random = new();

        [Test]
        public void IntMatcherTest([Values(-1,1)]int sign)
        {
            var needle = sign * Random.Next();
            var abc = TypeParse.Parse<int>(needle.ToString());
            abc.Should().Be(needle);
        }

        [Test]
        public void StringMatcherTest()
        {
            var needle = RandomString();
            var abc = TypeParse.Parse<string>(needle);
            abc.Should().Be(needle);
        }

        [Test]
        public void CharMatcherTest()
        {
            var needle = RandomChar;
            var abc = TypeParse.Parse<char>(needle.ToString());
            abc.Should().Be(needle);
        }

        [Test]
        public void IntListTest()
        {
            var result = TypeParse.Parse<IReadOnlyList<int>>("123 456\t789 \n\t000 101112");
            result.Should().Equal(123, 456, 789, 0, 101112);
        }

        [Test]
        public void CharListTest()
        {
            var result = TypeParse.Parse<IReadOnlyList<char>>("a b c");
            result.Should().Equal('a', 'b', 'c');
        }

        [Test]
        public void TupleTest()
        {
            var result = TypeParse.Parse<(string, int)>("abc 123");
            result.Should().Be(("abc", 123));
        }

        [Test]
        public void ListOfTuplesTest()
        {
            var result = TypeParse.Parse<List<(string, int)>>("abc 123, def 456", new FormatAttribute{Separator = ","});
            result.Should().Equal(("abc", 123), ("def", 456));
        }

        [Test]
        public void ListOfTuplesWithOptionalTest()
        {
            // TODO: doesn't work for (string, int?, int)
            var result = TypeParse.Parse<List<(string, int, int?)>>("abc 123, def 5 456", new FormatAttribute { Separator = "," });
            result.Should().Equal(("abc", 123, null), ("def", 5, 456));
        }

        [TestCase(0, 1, 0, 0)]
        [TestCase(0, 1, 1, 1)]
        [TestCase(1, 1, 1, 1)]
        [TestCase(1, 1, 2, 1)]
        [TestCase(2, 5, 2, 2)]
        [TestCase(2, 5, 10, 5)]
        public void ListTest(int min, int max, int generated, int expected)
        {
            var haystack = Enumerable.Range(0, generated).Join(" ");
            var matcher = TypeParse.GetTypeParser<List<int>>(new FormatAttribute(){ Min = min, Max = max });
            var m = matcher.Match(haystack);
            m.Should().NotBeNull();
            m!.Value.Should().Equal(Enumerable.Range(0, expected));
            var remainder = TypeParse.GetTypeParser<List<int>>(new FormatAttribute { Min = 0 }).Match(m.Remainder);
            remainder!.Value.Should().Equal(Enumerable.Range(expected, generated - expected));
        }

        [TestCase("", null)]
        [TestCase("abc", null)]
        [TestCase("abc 123", null)]
        [TestCase("123", 123)]
        [TestCase("   \t\n 123abc", 123)]
        public void NegativeMatch(string input, int? expected)
        {
            var matcher = TypeParse.GetTypeParser<int>();
            var m = matcher.Match(input);
            if (expected is { } e) m!.Value.Should().Be(e);
            else m.Should().BeNull();
        }

        [TestCase("", null)]
        [TestCase(" a", 'a')]
        [TestCase("a", 'a')]
        [TestCase(" b", 'b')]
        [TestCase("b", 'b')]
        [TestCase("cb", null)]
        public void OverrideRegexTest(string input, char? expected)
        {
            var matcher = TypeParse.GetTypeParser<char>(new FormatAttribute { Regex = "/a|b/" });
            var m = matcher.Match(input);
            if (expected is { } e) m!.Value.Should().Be(e);
            else m.Should().BeNull();
        }

        [Test]
        public void AlternativeTest()
        {
            var matcher = TypeParse.Compile<IAlternative<int, string>>();
            var m = matcher.Match(" 123");
            m!.Value.Select(_ => "int", _ => "string").Should().Be("int");
            m = matcher.Match(" abc");
            m!.Value.Select(_ => "int", _ => "string").Should().Be("string");
        }

        private string RandomString()
        {
            var options = Enumerable.Range('a', 'z' - 'a' + 1)
                .Concat(Enumerable.Range('A', 'Z' - 'A' + 1))
                .Select(it => (char)it)
                .ToList();

            var length = Random.Next(1, 256);
            return Enumerable.Range(0, length).Select(_ => options[Random.Next(0, options.Count)]).Join();
        }

        private readonly IReadOnlyList<char> Chars = Enumerable.Range('a', 'z' - 'a' + 1)
            .Concat(Enumerable.Range('A', 'Z' - 'A' + 1))
            .Select(it => (char)it)
            .ToList();

        private char RandomChar => Chars[Random.Next(0, Chars.Count)];
    }
}
