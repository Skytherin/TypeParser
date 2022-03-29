using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Common.Utils;
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
            var abc = TypeCompiler.Parse<int>(needle.ToString());
            abc.Should().Be(needle);
        }

        [Test]
        public void StringMatcherTest()
        {
            var needle = RandomString();
            var abc = TypeCompiler.Parse<string>(needle);
            abc.Should().Be(needle);
        }

        [Test]
        public void CharMatcherTest()
        {
            var needle = RandomChar;
            var abc = TypeCompiler.Parse<char>(needle.ToString());
            abc.Should().Be(needle);
        }

        [Test]
        public void IntListTest()
        {
            var result = TypeCompiler.Parse<IReadOnlyList<int>>("123 456\t789 \n\t000 101112");
            result.Should().Equal(123, 456, 789, 0, 101112);
        }

        [Test]
        public void CharListTest()
        {
            var result = TypeCompiler.Parse<IReadOnlyList<char>>("a b c");
            result.Should().Equal('a', 'b', 'c');
        }

        [Test]
        public void TupleTest()
        {
            var result = TypeCompiler.Parse<(string, int)>("abc 123");
            result.Should().Be(("abc", 123));
        }

        [Test]
        public void ListOfTuplesTest()
        {
            var result = TypeCompiler.Parse<List<(string, int)>>("abc 123, def 456", new Format{Separator = ","});
            result.Should().Equal(("abc", 123), ("def", 456));
        }

        [Test]
        public void ListOfTuplesWithOptionalTest()
        {
            // TODO: doesn't work for (string, int?, int)
            var result = TypeCompiler.Parse<List<(string, int, int?)>>("abc 123, def 5 456", new Format { Separator = "," });
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
            var matcher = TypeCompiler.GetTypeParser<List<int>>(new Format(){ Min = min, Max = max });
            var m = matcher.Match(haystack);
            m.Should().NotBeNull();
            m!.Value.Should().Equal(Enumerable.Range(0, expected));
            var remainder = TypeCompiler.GetTypeParser<List<int>>(new Format { Min = 0 }).Match(m.Remainder);
            remainder!.Value.Should().Equal(Enumerable.Range(expected, generated - expected));
        }

        [TestCase("", null)]
        [TestCase("abc", null)]
        [TestCase("abc 123", null)]
        [TestCase("123", 123)]
        [TestCase("   \t\n 123abc", 123)]
        public void NegativeMatch(string input, int? expected)
        {
            var matcher = TypeCompiler.GetTypeParser<int>();
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
            var matcher = TypeCompiler.GetTypeParser<char>(new Format { Regex = "a|b" });
            var m = matcher.Match(input);
            if (expected is { } e) m!.Value.Should().Be(e);
            else m.Should().BeNull();
        }

        [Test]
        public void ChoicesTest()
        {
            var matcher = TypeCompiler.Compile<string>(new Format { Choices = "one two three"});
            matcher.Match("one")!.Value.Should().Be("one");
            matcher.Match("two")!.Value.Should().Be("two");
            matcher.Match("three")!.Value.Should().Be("three");
            matcher.Match("four").Should().BeNull();
        }

        [Test]
        public void TwoAlternativesTest()
        {
            var matcher = TypeCompiler.Compile<IAlternative<int, string>>();
            var m = matcher.Match(" 123");
            m!.Value.Select(_ => "int", _ => "string").Should().Be("int");
            m = matcher.Match(" abc");
            m!.Value.Select(_ => "int", _ => "string").Should().Be("string");
        }

        [TestCase("1", 1)]
        [TestCase("2", 1)]
        [TestCase("3", 2)]
        [TestCase("4", 2)]
        [TestCase("5", 3)]
        [TestCase("6", 3)]
        public void ThreeAlternativesTest(string input, int expected)
        {
            var matcher = TypeCompiler.Compile<IAlternative<OneTwo, ThreeFour, FiveSix>>();
            var m = matcher.Match(input);
            m!.Value.Select(_ => 1, _ => 2, _ => 3).Should().Be(expected);
        }

        private record OneTwo([Format(Choices = "1 2")]int Value);
        private record ThreeFour([Format(Choices = "3 4")] int Value);
        private record FiveSix([Format(Choices = "5 6")] int Value);

        private record BeforeAndAfter(
            [Format(After = "=")] string Key,
            [Format(Before = "'", After = "'")] string Value
            );

        [Test]
        public void BeforeAndAfterTest()
        {
            var matcher = TypeCompiler.Compile<BeforeAndAfter>();
            var m = matcher.Match("Key = 'Value'");
            m!.Value.Should().Be(new BeforeAndAfter("Key", "Value"));
        }

        [Test]
        public void ChoicesSanity()
        {
            Func<ITypeParser<string>> action = () => TypeCompiler.Compile<string>(new Format { Choices = "a ab"});
            action.Should().Throw<ApplicationException>().Which.Message.Should().Contain("Choice ab will never be matched");
        }

        [TestCase("{}")]
        [TestCase("{{}}")]
        [TestCase("{{},{}}")]
        [TestCase("{{},{},{}}")]
        [TestCase("{{{}}}")]
        [TestCase("{{{},{}}}")]
        [TestCase("{{{},{},{}}}")]
        [TestCase("{{{},{},{{}}}}")]
        public void Nested(string input)
        {
            var m = TypeCompiler.Compile<NestableGroup>();
            m.Match(input).Should().NotBeNull();
        }

        [Test]
        public void RegexTest()
        {
            var m = TypeCompiler.Compile<string>(new Format { Regex = "<((!.)|[^!>])*>" });
            m.Match("<>").Should().NotBeNull();
        }

        [Test]
        public void CanCompileNestedRecursiveClass()
        {
            var m = TypeCompiler.Compile<Group>();
            m.Should().NotBeNull();
            m.Match("{<>}").Should().NotBeNull();
        }

        [Test]
        public void ListOfEnums()
        {
            var l = TypeCompiler.Parse<List<TestEnum>>("A B a b");
            l.Should().Equal(TestEnum.A, TestEnum.B, TestEnum.A, TestEnum.B);
        }

        [Test]
        public void DictionaryTest1()
        {
            var dm = TypeCompiler.Compile<IReadOnlyDictionary<string, int>>();
            var m = dm.Match(" a : 1 ");
            m.Should().NotBeNull();
            var d = m!.Value;
            d.Should().ContainKey("a").WhoseValue.Should().Be(1);
        }

        [Test]
        public void DictionaryTest2()
        {
            var dm = TypeCompiler.Compile<IReadOnlyDictionary<int, int>>();
            var m = dm.Match(@"0: 3
1: 2
4: 4
6: 4");
            m.Should().NotBeNull();
            var d = m!.Value;
            d.Should().ContainKey(0).WhoseValue.Should().Be(3);
            d.Should().ContainKey(1).WhoseValue.Should().Be(2);
            d.Should().ContainKey(4).WhoseValue.Should().Be(4);
            d.Should().ContainKey(6).WhoseValue.Should().Be(4);
        }

        [Test]
        public void DictionaryTest3()
        {
            var dm = TypeCompiler.Compile<IReadOnlyDictionary<int, int>>(new Format{Separator = ","});
            var m = dm.Match(@"0: 3,
1: 2,
4: 4,
6: 4");
            m.Should().NotBeNull();
            var d = m!.Value;
            d.Should().ContainKey(0).WhoseValue.Should().Be(3);
            d.Should().ContainKey(1).WhoseValue.Should().Be(2);
            d.Should().ContainKey(4).WhoseValue.Should().Be(4);
            d.Should().ContainKey(6).WhoseValue.Should().Be(4);
        }

        private enum TestEnum
        {
            A,
            B
        };

        private record Group(
            [Format(Before = "{", After = "}", Separator = ",")]
            IReadOnlyList<Subgroup> Subgroups
        );

        private record Subgroup(IAlternative<Group, Garbage> Content);

        private record Garbage(
            [Format(Regex = @"<((!.)|[^!>])*>")] string Content
        );

        private record NestableGroup(
            [Format(Before = "{", After = "}", Separator = ",")]
            IReadOnlyList<NestableGroup> Subgroups
        );

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
