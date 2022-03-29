using FluentAssertions;
using NUnit.Framework;
using TypeParser;
using TypeParser.UtilityClasses;

namespace UnitTests
{
    [TestFixture]
    public class MathWithAlternativesTest
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
            var matcher = TypeCompiler.Compile<Expression>();
            var m = matcher.Match(input);
            if (expected != null) m!.Value.Evaluate().Should().Be(expected);
            else m.Should().BeNull();
        }

        // Math1:
        // Expression ::= Expression2 AddTail?
        // Expression2 ::= ValueOrParened MultiplyTail?
        // AddExpression ::= Operator Expression
        internal class Expression
        {
            public Expression(Expression2 expression2,
                [Format(Optional = true)] AddTail? tail)
            {
                Expression2 = expression2;
                Tail = tail;
            }

            private readonly Expression2 Expression2;
            private readonly AddTail? Tail;

            public long Evaluate()
            {
                var v1 = Expression2.Evaluate();
                return Tail?.Evaluate(v1) ?? v1;
            }
        }

        internal class Expression2
        {
            public Expression2(
                IAlternative<ParenthesizedExpression, long> value,
                [Format(Optional = true)] MultiplyTail? tail)
            {
                Value = value;
                Tail = tail;
            }

            private readonly IAlternative<ParenthesizedExpression, long> Value;
            private readonly MultiplyTail? Tail;

            public long Evaluate()
            {
                var v1 = Value.Select(first => first.Expression.Evaluate(), second => second);
                return Tail?.Evaluate(v1) ?? v1;
            }
        }

        internal record ParenthesizedExpression([Format(Before = "(", After = ")")] Expression Expression);

        internal class AddTail
        {
            public AddTail([Format(Regex = @"[-+]")] char @operator,
                Expression expression)
            {
                Operator = @operator;
                Expression = expression;
            }

            private readonly char Operator;
            private readonly Expression Expression;

            public long Evaluate(long lhs)
            {
                return Operator == '+'
                    ? lhs + Expression.Evaluate()
                    : lhs - Expression.Evaluate();
            }
        }

        internal class MultiplyTail
        {
            public MultiplyTail([Format(Regex = @"[*/]")] char @operator,
                Expression2 expression)
            {
                Operator = @operator;
                Expression = expression;
            }

            public char Operator { get; init; }
            public Expression2 Expression { get; init; }

            public long Evaluate(long lhs)
            {
                return Operator == '*'
                    ? lhs * Expression.Evaluate()
                    : lhs / Expression.Evaluate();
            }
        }
    }
}