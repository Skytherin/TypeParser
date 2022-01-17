using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using Common.Utils;
using JetBrains.Annotations;
using TypeParser;


namespace UnitTests
{
    [TestFixture]
    public class Tests
    {
        private readonly Random Random = new();

        [Test]
        public void Test1([Values(-1,1)]int sign)
        {
            var needle = sign * Random.Next();
            var abc = TypeParser.TypeParser.Parse<int>(needle.ToString());
            abc.Should().Be(needle);
        }

        [Test]
        public void Test2()
        {
            var needle = RandomString();
            var abc = TypeParser.TypeParser.Parse<string>(needle);
            abc.Should().Be(needle);
        }

        [Test]
        public void Test3()
        {
            var needle = RandomChar;
            var abc = TypeParser.TypeParser.Parse<char>(needle.ToString());
            abc.Should().Be(needle);
        }

        [Test]
        public void IntListTest()
        {
            var result = TypeParser.TypeParser.Parse<IReadOnlyList<int>>("123 456\t789 \n\t000 101112");
            result.Should().Equal(123, 456, 789, 0, 101112);
        }

        [Test]
        public void CharListTest()
        {
            var result = TypeParser.TypeParser.Parse<IReadOnlyList<char>>("a b c");
            result.Should().Equal('a', 'b', 'c');
        }

        [Test]
        public void AnotherTest()
        {
            var result = TypeParser.TypeParser.Parse<(string, int)>("abc 123");
            result.Should().Be(("abc", 123));
        }

        [UsedImplicitly]
        private record TestClass([RxRepeat(Separator = ",")]List<(string, int)> Value);

        [Test]
        public void AnotherTest2()
        {
            var result = TypeParser.TypeParser.Parse<TestClass>("abc 123, def 456");
            result.Value.Should().Equal(("abc", 123), ("def", 456));
        }

        [Test]
        public void Test4()
        {
            var result = TypeParser.TypeParser.ParseOrDefault<DayClass>("Day01");
        }

        [TestCase(1, 1, 1, 1)]
        public void RepeatTest(int min, int max, int generated, int expected)
        {
            var haystack = Enumerable.Range(0, generated).Join(" ");
            var matcher = TypeParser.TypeParser.Compile<List<int>>(repeat: new RxRepeat { Min = min, Max = max });
            var m = matcher.TryScan(haystack, out var result, out _);
        }

        [Inherited]
        public record RecordWithInheritedAttribute(int Value);

        public class DayClass
        {
            [RxFormat(Before = "Day")]
            public int DayNumber { get; set; }
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

    [AttributeUsage(AttributeTargets.All)]
    public class InheritedAttribute : RxFormat
    {

    }
}
