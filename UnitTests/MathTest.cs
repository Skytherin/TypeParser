using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using JetBrains.Annotations;
using NUnit.Framework;
using TypeParser;

namespace UnitTests
{
    [TestFixture]
    public class MathTest
    {
        [TestCase("1", 1)]
        [TestCase("(1)", 1)]
        [TestCase("1 + 2", 3)]
        [TestCase("(1 + 2)", 3)]
        [TestCase("(1) + 2", 3)]
        [TestCase("2 + 3 * 4", 14)]
        [TestCase("4 * 3 + 2", 14)]
        [TestCase("( 2 + 3 ) * 4", 20)]
        public void MathTest1(string input, long? expected)
        {
            var matcher = TypeParse.Compile<Expression>();
            var m = matcher.Match(input);
            if (expected != null) m.Value.Evaluate().Should().Be(expected);
            else m.Should().BeNull();
        }
    }

    // Math1:
    // Expression ::= Expression2 AddTail?
    // Expression2 ::= ValueOrParened MultiplyTail?
    // AddExpression ::= Operator Expression
    internal record Expression(
        Expression2 Expression2,
        [Format(Optional = true)] AddTail? Tail);

    internal record Expression2(
        [RxAlternate, Format(Before = "(", After = ")")] Expression? ParenExpression,
        [RxAlternate] long? Value,
        [Format(Optional = true)] MultiplyTail? Tail);

    internal record AddTail([Format(Regex = @"/[-+]/")] char Operator,
        Expression Expression);

    internal record MultiplyTail([Format(Regex = @"/[*/]/")] char Operator,
        Expression2 Expression);

    public static class ExpressionExtensions
    {
        internal static long Evaluate(this Expression self)
        {
            var v1 = self.Expression2.Evaluate();
            return self.Tail?.Evaluate(v1) ?? v1;
        }

        internal static long Evaluate(this Expression2 self)
        {
            var v1 = self.ParenExpression?.Evaluate() ?? self.Value ?? throw new ApplicationException();
            return self.Tail?.Evaluate(v1) ?? v1;
        }

        internal static long Evaluate(this AddTail self, long lhs)
        {
            return self.Operator == '+'
                ? lhs + self.Expression.Evaluate()
                : lhs - self.Expression.Evaluate();
        }

        internal static long Evaluate(this MultiplyTail self, long lhs)
        {
            return self.Operator == '*'
                ? lhs * self.Expression.Evaluate()
                : lhs / self.Expression.Evaluate();
        }
    }
}