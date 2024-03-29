﻿using System;
using FluentAssertions;
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
                this.Expression2 = expression2;
                this.Tail = tail;
            }

            public Expression2 Expression2 { get; init; }
            public AddTail? Tail { get; init; }

            public long Evaluate()
            {
                var v1 = Expression2.Evaluate();
                return Tail?.Evaluate(v1) ?? v1;
            }
        }

        internal class Expression2
        {
            public Expression2([Alternate, Format(Before = "(", After = ")")] Expression? parenExpression,
                [Alternate] long? value,
                [Format(Optional = true)] MultiplyTail? tail)
            {
                ParenExpression = parenExpression;
                Value = value;
                Tail = tail;
            }

            public Expression? ParenExpression { get; init; }
            public long? Value { get; init; }
            public MultiplyTail? Tail { get; init; }

            public long Evaluate()
            {
                var v1 = ParenExpression?.Evaluate() ?? Value ?? throw new ApplicationException();
                return Tail?.Evaluate(v1) ?? v1;
            }
        }

        internal class AddTail
        {
            public AddTail([Format(Regex = "[-+]")] char @operator,
                Expression expression)
            {
                Operator = @operator;
                Expression = expression;
            }

            public char Operator { get; init; }
            public Expression Expression { get; init; }

            public long Evaluate(long lhs)
            {
                return Operator == '+'
                    ? lhs + Expression.Evaluate()
                    : lhs - Expression.Evaluate();
            }
        }

        internal class MultiplyTail
        {
            public MultiplyTail([Format(Regex = "[*/]")] char @operator,
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