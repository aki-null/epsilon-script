using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;
using static EpsilonScript.Tests.TestInfrastructure.ElementFactory;

namespace EpsilonScript.Tests.Parser
{
  public class ParserComplexExpressionTests : TokenParserTestBase
  {
    [Theory]
    [MemberData(nameof(DeeplyNestedExpressions))]
    internal void Parser_DeeplyNestedExpressions_ParsesCorrectly(Token[] input, Element[] expected)
    {
      AssertParseSucceeds(input, expected);
    }

    [Theory]
    [MemberData(nameof(ComplexArithmeticExpressions))]
    internal void Parser_ComplexArithmeticExpressions_ParsesCorrectly(Token[] input, Element[] expected)
    {
      AssertParseSucceeds(input, expected);
    }

    [Theory]
    [MemberData(nameof(ComplexBooleanExpressions))]
    internal void Parser_ComplexBooleanExpressions_ParsesCorrectly(Token[] input, Element[] expected)
    {
      AssertParseSucceeds(input, expected);
    }

    [Theory]
    [MemberData(nameof(ComplexFunctionCallExpressions))]
    internal void Parser_ComplexFunctionCallExpressions_ParsesCorrectly(Token[] input, Element[] expected)
    {
      AssertParseSucceeds(input, expected);
    }

    [Theory]
    [MemberData(nameof(MixedTypeExpressions))]
    internal void Parser_MixedTypeExpressions_ParsesCorrectly(Token[] input, Element[] expected)
    {
      AssertParseSucceeds(input, expected);
    }

    public static IEnumerable<object[]> DeeplyNestedExpressions
    {
      get
      {
        return new[]
        {
          // ((1 + 2) * (3 + 4))
          CreateTestCase(
            new[]
            {
              new Token("(", TokenType.LeftParenthesis),
              new Token("(", TokenType.LeftParenthesis),
              new Token("1", TokenType.Integer),
              new Token("+", TokenType.PlusSign),
              new Token("2", TokenType.Integer),
              new Token(")", TokenType.RightParenthesis),
              new Token("*", TokenType.MultiplyOperator),
              new Token("(", TokenType.LeftParenthesis),
              new Token("3", TokenType.Integer),
              new Token("+", TokenType.PlusSign),
              new Token("4", TokenType.Integer),
              new Token(")", TokenType.RightParenthesis),
              new Token(")", TokenType.RightParenthesis)
            },
            Create("(", TokenType.LeftParenthesis, ElementType.LeftParenthesis),
            Create("(", TokenType.LeftParenthesis, ElementType.LeftParenthesis),
            Create("1", TokenType.Integer, ElementType.Integer),
            Create("+", TokenType.PlusSign, ElementType.AddOperator),
            Create("2", TokenType.Integer, ElementType.Integer),
            Create(")", TokenType.RightParenthesis, ElementType.RightParenthesis),
            Create("*", TokenType.MultiplyOperator, ElementType.MultiplyOperator),
            Create("(", TokenType.LeftParenthesis, ElementType.LeftParenthesis),
            Create("3", TokenType.Integer, ElementType.Integer),
            Create("+", TokenType.PlusSign, ElementType.AddOperator),
            Create("4", TokenType.Integer, ElementType.Integer),
            Create(")", TokenType.RightParenthesis, ElementType.RightParenthesis),
            Create(")", TokenType.RightParenthesis, ElementType.RightParenthesis)
          ),

          // (((1)))
          CreateTestCase(
            new[]
            {
              new Token("(", TokenType.LeftParenthesis),
              new Token("(", TokenType.LeftParenthesis),
              new Token("(", TokenType.LeftParenthesis),
              new Token("1", TokenType.Integer),
              new Token(")", TokenType.RightParenthesis),
              new Token(")", TokenType.RightParenthesis),
              new Token(")", TokenType.RightParenthesis)
            },
            Create("(", TokenType.LeftParenthesis, ElementType.LeftParenthesis),
            Create("(", TokenType.LeftParenthesis, ElementType.LeftParenthesis),
            Create("(", TokenType.LeftParenthesis, ElementType.LeftParenthesis),
            Create("1", TokenType.Integer, ElementType.Integer),
            Create(")", TokenType.RightParenthesis, ElementType.RightParenthesis),
            Create(")", TokenType.RightParenthesis, ElementType.RightParenthesis),
            Create(")", TokenType.RightParenthesis, ElementType.RightParenthesis)
          )
        };
      }
    }

    public static IEnumerable<object[]> ComplexArithmeticExpressions
    {
      get
      {
        return new[]
        {
          // 1 + 2 * 3 - 4 / 5 % 6
          CreateTestCase(
            new[]
            {
              new Token("1", TokenType.Integer),
              new Token("+", TokenType.PlusSign),
              new Token("2", TokenType.Integer),
              new Token("*", TokenType.MultiplyOperator),
              new Token("3", TokenType.Integer),
              new Token("-", TokenType.MinusSign),
              new Token("4", TokenType.Integer),
              new Token("/", TokenType.DivideOperator),
              new Token("5", TokenType.Integer),
              new Token("%", TokenType.ModuloOperator),
              new Token("6", TokenType.Integer)
            },
            Create("1", TokenType.Integer, ElementType.Integer),
            Create("+", TokenType.PlusSign, ElementType.AddOperator),
            Create("2", TokenType.Integer, ElementType.Integer),
            Create("*", TokenType.MultiplyOperator, ElementType.MultiplyOperator),
            Create("3", TokenType.Integer, ElementType.Integer),
            Create("-", TokenType.MinusSign, ElementType.SubtractOperator),
            Create("4", TokenType.Integer, ElementType.Integer),
            Create("/", TokenType.DivideOperator, ElementType.DivideOperator),
            Create("5", TokenType.Integer, ElementType.Integer),
            Create("%", TokenType.ModuloOperator, ElementType.ModuloOperator),
            Create("6", TokenType.Integer, ElementType.Integer)
          ),

          // -(-(-1))
          CreateTestCase(
            new[]
            {
              new Token("-", TokenType.MinusSign),
              new Token("(", TokenType.LeftParenthesis),
              new Token("-", TokenType.MinusSign),
              new Token("(", TokenType.LeftParenthesis),
              new Token("-", TokenType.MinusSign),
              new Token("1", TokenType.Integer),
              new Token(")", TokenType.RightParenthesis),
              new Token(")", TokenType.RightParenthesis)
            },
            Create("-", TokenType.MinusSign, ElementType.NegativeOperator),
            Create("(", TokenType.LeftParenthesis, ElementType.LeftParenthesis),
            Create("-", TokenType.MinusSign, ElementType.NegativeOperator),
            Create("(", TokenType.LeftParenthesis, ElementType.LeftParenthesis),
            Create("-", TokenType.MinusSign, ElementType.NegativeOperator),
            Create("1", TokenType.Integer, ElementType.Integer),
            Create(")", TokenType.RightParenthesis, ElementType.RightParenthesis),
            Create(")", TokenType.RightParenthesis, ElementType.RightParenthesis)
          )
        };
      }
    }

    public static IEnumerable<object[]> ComplexBooleanExpressions
    {
      get
      {
        return new[]
        {
          // true && (false || true) && !false
          CreateTestCase(
            new[]
            {
              new Token("true", TokenType.BooleanLiteralTrue),
              new Token("&&", TokenType.BooleanAndOperator),
              new Token("(", TokenType.LeftParenthesis),
              new Token("false", TokenType.BooleanLiteralFalse),
              new Token("||", TokenType.BooleanOrOperator),
              new Token("true", TokenType.BooleanLiteralTrue),
              new Token(")", TokenType.RightParenthesis),
              new Token("&&", TokenType.BooleanAndOperator),
              new Token("!", TokenType.NegateOperator),
              new Token("false", TokenType.BooleanLiteralFalse)
            },
            Create("true", TokenType.BooleanLiteralTrue, ElementType.BooleanLiteralTrue),
            Create("&&", TokenType.BooleanAndOperator, ElementType.BooleanAndOperator),
            Create("(", TokenType.LeftParenthesis, ElementType.LeftParenthesis),
            Create("false", TokenType.BooleanLiteralFalse, ElementType.BooleanLiteralFalse),
            Create("||", TokenType.BooleanOrOperator, ElementType.BooleanOrOperator),
            Create("true", TokenType.BooleanLiteralTrue, ElementType.BooleanLiteralTrue),
            Create(")", TokenType.RightParenthesis, ElementType.RightParenthesis),
            Create("&&", TokenType.BooleanAndOperator, ElementType.BooleanAndOperator),
            Create("!", TokenType.NegateOperator, ElementType.NegateOperator),
            Create("false", TokenType.BooleanLiteralFalse, ElementType.BooleanLiteralFalse)
          ),

          // 1 < 2 && 3 >= 4 || 5 != 6
          CreateTestCase(
            new[]
            {
              new Token("1", TokenType.Integer),
              new Token("<", TokenType.ComparisonLessThan),
              new Token("2", TokenType.Integer),
              new Token("&&", TokenType.BooleanAndOperator),
              new Token("3", TokenType.Integer),
              new Token(">=", TokenType.ComparisonGreaterThanOrEqualTo),
              new Token("4", TokenType.Integer),
              new Token("||", TokenType.BooleanOrOperator),
              new Token("5", TokenType.Integer),
              new Token("!=", TokenType.ComparisonNotEqual),
              new Token("6", TokenType.Integer)
            },
            Create("1", TokenType.Integer, ElementType.Integer),
            Create("<", TokenType.ComparisonLessThan, ElementType.ComparisonLessThan),
            Create("2", TokenType.Integer, ElementType.Integer),
            Create("&&", TokenType.BooleanAndOperator, ElementType.BooleanAndOperator),
            Create("3", TokenType.Integer, ElementType.Integer),
            Create(">=", TokenType.ComparisonGreaterThanOrEqualTo, ElementType.ComparisonGreaterThanOrEqualTo),
            Create("4", TokenType.Integer, ElementType.Integer),
            Create("||", TokenType.BooleanOrOperator, ElementType.BooleanOrOperator),
            Create("5", TokenType.Integer, ElementType.Integer),
            Create("!=", TokenType.ComparisonNotEqual, ElementType.ComparisonNotEqual),
            Create("6", TokenType.Integer, ElementType.Integer)
          ),

          // Test unary operators after comparison operators: 1 < -2 && 10 > +5
          CreateTestCase(
            new[]
            {
              new Token("1", TokenType.Integer),
              new Token("<", TokenType.ComparisonLessThan),
              new Token("-", TokenType.MinusSign),
              new Token("2", TokenType.Integer),
              new Token("&&", TokenType.BooleanAndOperator),
              new Token("10", TokenType.Integer),
              new Token(">", TokenType.ComparisonGreaterThan),
              new Token("+", TokenType.PlusSign),
              new Token("5", TokenType.Integer)
            },
            Create("1", TokenType.Integer, ElementType.Integer),
            Create("<", TokenType.ComparisonLessThan, ElementType.ComparisonLessThan),
            Create("-", TokenType.MinusSign, ElementType.NegativeOperator),
            Create("2", TokenType.Integer, ElementType.Integer),
            Create("&&", TokenType.BooleanAndOperator, ElementType.BooleanAndOperator),
            Create("10", TokenType.Integer, ElementType.Integer),
            Create(">", TokenType.ComparisonGreaterThan, ElementType.ComparisonGreaterThan),
            Create("+", TokenType.PlusSign, ElementType.PositiveOperator),
            Create("5", TokenType.Integer, ElementType.Integer)
          )
        };
      }
    }

    public static IEnumerable<object[]> ComplexFunctionCallExpressions
    {
      get
      {
        return new[]
        {
          // func1(func2(1, 2), func3())
          CreateTestCase(
            new[]
            {
              new Token("func1", TokenType.Identifier),
              new Token("(", TokenType.LeftParenthesis),
              new Token("func2", TokenType.Identifier),
              new Token("(", TokenType.LeftParenthesis),
              new Token("1", TokenType.Integer),
              new Token(",", TokenType.Comma),
              new Token("2", TokenType.Integer),
              new Token(")", TokenType.RightParenthesis),
              new Token(",", TokenType.Comma),
              new Token("func3", TokenType.Identifier),
              new Token("(", TokenType.LeftParenthesis),
              new Token(")", TokenType.RightParenthesis),
              new Token(")", TokenType.RightParenthesis)
            },
            Create("func1", TokenType.Identifier, ElementType.Function),
            Create("(", TokenType.LeftParenthesis, ElementType.FunctionStartParenthesis),
            Create("func2", TokenType.Identifier, ElementType.Function),
            Create("(", TokenType.LeftParenthesis, ElementType.FunctionStartParenthesis),
            Create("1", TokenType.Integer, ElementType.Integer),
            Create(",", TokenType.Comma, ElementType.Comma),
            Create("2", TokenType.Integer, ElementType.Integer),
            Create(")", TokenType.RightParenthesis, ElementType.RightParenthesis),
            Create(",", TokenType.Comma, ElementType.Comma),
            Create("func3", TokenType.Identifier, ElementType.Function),
            Create("(", TokenType.LeftParenthesis, ElementType.FunctionStartParenthesis),
            Create("", TokenType.None, ElementType.None),
            Create(")", TokenType.RightParenthesis, ElementType.RightParenthesis),
            Create(")", TokenType.RightParenthesis, ElementType.RightParenthesis)
          ),

          // func(1 + 2, 3 * 4, func2(5))
          CreateTestCase(
            new[]
            {
              new Token("func", TokenType.Identifier),
              new Token("(", TokenType.LeftParenthesis),
              new Token("1", TokenType.Integer),
              new Token("+", TokenType.PlusSign),
              new Token("2", TokenType.Integer),
              new Token(",", TokenType.Comma),
              new Token("3", TokenType.Integer),
              new Token("*", TokenType.MultiplyOperator),
              new Token("4", TokenType.Integer),
              new Token(",", TokenType.Comma),
              new Token("func2", TokenType.Identifier),
              new Token("(", TokenType.LeftParenthesis),
              new Token("5", TokenType.Integer),
              new Token(")", TokenType.RightParenthesis),
              new Token(")", TokenType.RightParenthesis)
            },
            Create("func", TokenType.Identifier, ElementType.Function),
            Create("(", TokenType.LeftParenthesis, ElementType.FunctionStartParenthesis),
            Create("1", TokenType.Integer, ElementType.Integer),
            Create("+", TokenType.PlusSign, ElementType.AddOperator),
            Create("2", TokenType.Integer, ElementType.Integer),
            Create(",", TokenType.Comma, ElementType.Comma),
            Create("3", TokenType.Integer, ElementType.Integer),
            Create("*", TokenType.MultiplyOperator, ElementType.MultiplyOperator),
            Create("4", TokenType.Integer, ElementType.Integer),
            Create(",", TokenType.Comma, ElementType.Comma),
            Create("func2", TokenType.Identifier, ElementType.Function),
            Create("(", TokenType.LeftParenthesis, ElementType.FunctionStartParenthesis),
            Create("5", TokenType.Integer, ElementType.Integer),
            Create(")", TokenType.RightParenthesis, ElementType.RightParenthesis),
            Create(")", TokenType.RightParenthesis, ElementType.RightParenthesis)
          ),

          // Test function call followed by operators: func() + 1, func() * -2
          CreateTestCase(
            new[]
            {
              new Token("func", TokenType.Identifier),
              new Token("(", TokenType.LeftParenthesis),
              new Token(")", TokenType.RightParenthesis),
              new Token("+", TokenType.PlusSign),
              new Token("1", TokenType.Integer),
              new Token("*", TokenType.MultiplyOperator),
              new Token("-", TokenType.MinusSign),
              new Token("2", TokenType.Integer)
            },
            Create("func", TokenType.Identifier, ElementType.Function),
            Create("(", TokenType.LeftParenthesis, ElementType.FunctionStartParenthesis),
            Create("", TokenType.None, ElementType.None),
            Create(")", TokenType.RightParenthesis, ElementType.RightParenthesis),
            Create("+", TokenType.PlusSign, ElementType.AddOperator),
            Create("1", TokenType.Integer, ElementType.Integer),
            Create("*", TokenType.MultiplyOperator, ElementType.MultiplyOperator),
            Create("-", TokenType.MinusSign, ElementType.NegativeOperator),
            Create("2", TokenType.Integer, ElementType.Integer)
          )
        };
      }
    }

    public static IEnumerable<object[]> MixedTypeExpressions
    {
      get
      {
        return new[]
        {
          // "Hello " + (1 + 2) + " World"
          CreateTestCase(
            new[]
            {
              new Token("\"Hello \"", TokenType.String),
              new Token("+", TokenType.PlusSign),
              new Token("(", TokenType.LeftParenthesis),
              new Token("1", TokenType.Integer),
              new Token("+", TokenType.PlusSign),
              new Token("2", TokenType.Integer),
              new Token(")", TokenType.RightParenthesis),
              new Token("+", TokenType.PlusSign),
              new Token("\" World\"", TokenType.String)
            },
            Create("\"Hello \"", TokenType.String, ElementType.String),
            Create("+", TokenType.PlusSign, ElementType.AddOperator),
            Create("(", TokenType.LeftParenthesis, ElementType.LeftParenthesis),
            Create("1", TokenType.Integer, ElementType.Integer),
            Create("+", TokenType.PlusSign, ElementType.AddOperator),
            Create("2", TokenType.Integer, ElementType.Integer),
            Create(")", TokenType.RightParenthesis, ElementType.RightParenthesis),
            Create("+", TokenType.PlusSign, ElementType.AddOperator),
            Create("\" World\"", TokenType.String, ElementType.String)
          ),

          // x = func(1.5 + 2) * 3; y = x > 5
          CreateTestCase(
            new[]
            {
              new Token("x", TokenType.Identifier),
              new Token("=", TokenType.AssignmentOperator),
              new Token("func", TokenType.Identifier),
              new Token("(", TokenType.LeftParenthesis),
              new Token("1.5", TokenType.Float),
              new Token("+", TokenType.PlusSign),
              new Token("2", TokenType.Integer),
              new Token(")", TokenType.RightParenthesis),
              new Token("*", TokenType.MultiplyOperator),
              new Token("3", TokenType.Integer),
              new Token(";", TokenType.Semicolon),
              new Token("y", TokenType.Identifier),
              new Token("=", TokenType.AssignmentOperator),
              new Token("x", TokenType.Identifier),
              new Token(">", TokenType.ComparisonGreaterThan),
              new Token("5", TokenType.Integer)
            },
            Create("x", TokenType.Identifier, ElementType.Variable),
            Create("=", TokenType.AssignmentOperator, ElementType.AssignmentOperator),
            Create("func", TokenType.Identifier, ElementType.Function),
            Create("(", TokenType.LeftParenthesis, ElementType.FunctionStartParenthesis),
            Create("1.5", TokenType.Float, ElementType.Float),
            Create("+", TokenType.PlusSign, ElementType.AddOperator),
            Create("2", TokenType.Integer, ElementType.Integer),
            Create(")", TokenType.RightParenthesis, ElementType.RightParenthesis),
            Create("*", TokenType.MultiplyOperator, ElementType.MultiplyOperator),
            Create("3", TokenType.Integer, ElementType.Integer),
            Create(";", TokenType.Semicolon, ElementType.Semicolon),
            Create("y", TokenType.Identifier, ElementType.Variable),
            Create("=", TokenType.AssignmentOperator, ElementType.AssignmentOperator),
            Create("x", TokenType.Identifier, ElementType.Variable),
            Create(">", TokenType.ComparisonGreaterThan, ElementType.ComparisonGreaterThan),
            Create("5", TokenType.Integer, ElementType.Integer)
          )
        };
      }
    }

    // Helper method to create test cases
    private static object[] CreateTestCase(Token[] tokens, params Element[] expected)
    {
      return new object[] { tokens, expected };
    }
  }
}