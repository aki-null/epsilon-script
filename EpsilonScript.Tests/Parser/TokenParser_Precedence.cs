using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;
using static EpsilonScript.Tests.TestInfrastructure.ElementFactory;

namespace EpsilonScript.Tests.Parser
{
  public class TokenParser_Precedence : TokenParserTestBase
  {
    [Theory]
    [MemberData(nameof(PrecedenceLevel8Data))]
    internal void TokenParser_PrecedenceLevel8_FunctionAndUnaryOperators(Token[] input, Element[] expected)
    {
      AssertParseSucceeds(input, expected);
    }

    [Theory]
    [MemberData(nameof(PrecedenceLevel7Data))]
    internal void TokenParser_PrecedenceLevel7_MultiplicationDivisionModulo(Token[] input, Element[] expected)
    {
      AssertParseSucceeds(input, expected);
    }

    [Theory]
    [MemberData(nameof(PrecedenceLevel6Data))]
    internal void TokenParser_PrecedenceLevel6_AdditionSubtraction(Token[] input, Element[] expected)
    {
      AssertParseSucceeds(input, expected);
    }

    [Theory]
    [MemberData(nameof(PrecedenceLevel5Data))]
    internal void TokenParser_PrecedenceLevel5_ComparisonOperators(Token[] input, Element[] expected)
    {
      AssertParseSucceeds(input, expected);
    }

    [Theory]
    [MemberData(nameof(PrecedenceLevel4Data))]
    internal void TokenParser_PrecedenceLevel4_BooleanAnd(Token[] input, Element[] expected)
    {
      AssertParseSucceeds(input, expected);
    }

    [Theory]
    [MemberData(nameof(PrecedenceLevel3Data))]
    internal void TokenParser_PrecedenceLevel3_BooleanOr(Token[] input, Element[] expected)
    {
      AssertParseSucceeds(input, expected);
    }

    [Theory]
    [MemberData(nameof(PrecedenceLevel2Data))]
    internal void TokenParser_PrecedenceLevel2_AssignmentOperators(Token[] input, Element[] expected)
    {
      AssertParseSucceeds(input, expected);
    }

    [Theory]
    [MemberData(nameof(PrecedenceLevel1Data))]
    internal void TokenParser_PrecedenceLevel1_Comma(Token[] input, Element[] expected)
    {
      AssertParseSucceeds(input, expected);
    }

    [Theory]
    [MemberData(nameof(PrecedenceLevel0Data))]
    internal void TokenParser_PrecedenceLevel0_Semicolon(Token[] input, Element[] expected)
    {
      AssertParseSucceeds(input, expected);
    }

    [Theory]
    [MemberData(nameof(MixedPrecedenceData))]
    internal void TokenParser_MixedPrecedenceLevels_CorrectOrderOfOperations(Token[] input, Element[] expected)
    {
      AssertParseSucceeds(input, expected);
    }

    [Theory]
    [MemberData(nameof(AssociativityData))]
    internal void TokenParser_Associativity_CorrectEvaluation(Token[] input, Element[] expected)
    {
      AssertParseSucceeds(input, expected);
    }

    public static IEnumerable<object[]> PrecedenceLevel8Data
    {
      get
      {
        return new[]
        {
          // Function calls - precedence 8 (identifier followed by parentheses becomes function)
          CreateTestCase(
            new[]
            {
              new Token("func", TokenType.Identifier),
              new Token("(", TokenType.LeftParenthesis),
              new Token("1", TokenType.Integer),
              new Token(")", TokenType.RightParenthesis)
            },
            Create("func", TokenType.Identifier, ElementType.Function),
            Create("(", TokenType.LeftParenthesis, ElementType.FunctionStartParenthesis),
            Create("1", TokenType.Integer, ElementType.Integer),
            Create(")", TokenType.RightParenthesis, ElementType.RightParenthesis)
          ),
          // Unary negate operator - precedence 8 (negation comes before operand)
          CreateTestCase(
            new[] { new Token("!", TokenType.NegateOperator), new Token("true", TokenType.BooleanLiteralTrue) },
            Create("!", TokenType.NegateOperator, ElementType.NegateOperator),
            Create("true", TokenType.BooleanLiteralTrue, ElementType.BooleanLiteralTrue)
          ),
          // Unary negative operator - precedence 8 (negative comes before operand)
          CreateTestCase(
            new[] { new Token("-", TokenType.MinusSign), new Token("5", TokenType.Integer) },
            Create("-", TokenType.MinusSign, ElementType.NegativeOperator),
            Create("5", TokenType.Integer, ElementType.Integer)
          )
        };
      }
    }

    public static IEnumerable<object[]> PrecedenceLevel7Data
    {
      get
      {
        return new[]
        {
          // Multiplication, Division, Modulo - precedence 7
          CreateTestCase(
            new[]
            {
              new Token("2", TokenType.Integer), new Token("3", TokenType.Integer),
              new Token("*", TokenType.MultiplyOperator)
            },
            Create("2", TokenType.Integer, ElementType.Integer),
            Create("3", TokenType.Integer, ElementType.Integer),
            Create("*", TokenType.MultiplyOperator, ElementType.MultiplyOperator)
          ),
          CreateTestCase(
            new[]
            {
              new Token("8", TokenType.Integer), new Token("2", TokenType.Integer),
              new Token("/", TokenType.DivideOperator)
            },
            Create("8", TokenType.Integer, ElementType.Integer),
            Create("2", TokenType.Integer, ElementType.Integer),
            Create("/", TokenType.DivideOperator, ElementType.DivideOperator)
          ),
          CreateTestCase(
            new[]
            {
              new Token("7", TokenType.Integer), new Token("3", TokenType.Integer),
              new Token("%", TokenType.ModuloOperator)
            },
            Create("7", TokenType.Integer, ElementType.Integer),
            Create("3", TokenType.Integer, ElementType.Integer),
            Create("%", TokenType.ModuloOperator, ElementType.ModuloOperator)
          )
        };
      }
    }

    public static IEnumerable<object[]> PrecedenceLevel6Data
    {
      get
      {
        return new[]
        {
          // Addition, Subtraction - precedence 6
          CreateTestCase(
            new[]
            {
              new Token("1", TokenType.Integer), new Token("2", TokenType.Integer), new Token("+", TokenType.PlusSign)
            },
            Create("1", TokenType.Integer, ElementType.Integer),
            Create("2", TokenType.Integer, ElementType.Integer),
            Create("+", TokenType.PlusSign, ElementType.AddOperator)
          ),
          CreateTestCase(
            new[]
            {
              new Token("5", TokenType.Integer), new Token("3", TokenType.Integer), new Token("-", TokenType.MinusSign)
            },
            Create("5", TokenType.Integer, ElementType.Integer),
            Create("3", TokenType.Integer, ElementType.Integer),
            Create("-", TokenType.MinusSign, ElementType.SubtractOperator)
          )
        };
      }
    }

    public static IEnumerable<object[]> PrecedenceLevel5Data
    {
      get
      {
        return new[]
        {
          // Comparison operators - precedence 5
          CreateTestCase(
            new[]
            {
              new Token("1", TokenType.Integer), new Token("2", TokenType.Integer),
              new Token("==", TokenType.ComparisonEqual)
            },
            Create("1", TokenType.Integer, ElementType.Integer),
            Create("2", TokenType.Integer, ElementType.Integer),
            Create("==", TokenType.ComparisonEqual, ElementType.ComparisonEqual)
          ),
          CreateTestCase(
            new[]
            {
              new Token("1", TokenType.Integer), new Token("2", TokenType.Integer),
              new Token("!=", TokenType.ComparisonNotEqual)
            },
            Create("1", TokenType.Integer, ElementType.Integer),
            Create("2", TokenType.Integer, ElementType.Integer),
            Create("!=", TokenType.ComparisonNotEqual, ElementType.ComparisonNotEqual)
          ),
          CreateTestCase(
            new[]
            {
              new Token("1", TokenType.Integer), new Token("2", TokenType.Integer),
              new Token("<", TokenType.ComparisonLessThan)
            },
            Create("1", TokenType.Integer, ElementType.Integer),
            Create("2", TokenType.Integer, ElementType.Integer),
            Create("<", TokenType.ComparisonLessThan, ElementType.ComparisonLessThan)
          )
        };
      }
    }

    public static IEnumerable<object[]> PrecedenceLevel4Data
    {
      get
      {
        return new[]
        {
          // Boolean AND - precedence 4
          CreateTestCase(
            new[]
            {
              new Token("true", TokenType.BooleanLiteralTrue), new Token("false", TokenType.BooleanLiteralFalse),
              new Token("&&", TokenType.BooleanAndOperator)
            },
            Create("true", TokenType.BooleanLiteralTrue, ElementType.BooleanLiteralTrue),
            Create("false", TokenType.BooleanLiteralFalse, ElementType.BooleanLiteralFalse),
            Create("&&", TokenType.BooleanAndOperator, ElementType.BooleanAndOperator)
          )
        };
      }
    }

    public static IEnumerable<object[]> PrecedenceLevel3Data
    {
      get
      {
        return new[]
        {
          // Boolean OR - precedence 3
          CreateTestCase(
            new[]
            {
              new Token("true", TokenType.BooleanLiteralTrue), new Token("false", TokenType.BooleanLiteralFalse),
              new Token("||", TokenType.BooleanOrOperator)
            },
            Create("true", TokenType.BooleanLiteralTrue, ElementType.BooleanLiteralTrue),
            Create("false", TokenType.BooleanLiteralFalse, ElementType.BooleanLiteralFalse),
            Create("||", TokenType.BooleanOrOperator, ElementType.BooleanOrOperator)
          )
        };
      }
    }

    public static IEnumerable<object[]> PrecedenceLevel2Data
    {
      get
      {
        return new[]
        {
          // Assignment operators - precedence 2
          CreateTestCase(
            new[]
            {
              new Token("x", TokenType.Identifier), new Token("1", TokenType.Integer),
              new Token("=", TokenType.AssignmentOperator)
            },
            Create("x", TokenType.Identifier, ElementType.Variable),
            Create("1", TokenType.Integer, ElementType.Integer),
            Create("=", TokenType.AssignmentOperator, ElementType.AssignmentOperator)
          ),
          CreateTestCase(
            new[]
            {
              new Token("x", TokenType.Identifier), new Token("1", TokenType.Integer),
              new Token("+=", TokenType.AssignmentAddOperator)
            },
            Create("x", TokenType.Identifier, ElementType.Variable),
            Create("1", TokenType.Integer, ElementType.Integer),
            Create("+=", TokenType.AssignmentAddOperator, ElementType.AssignmentAddOperator)
          )
        };
      }
    }

    public static IEnumerable<object[]> PrecedenceLevel1Data
    {
      get
      {
        return new[]
        {
          // Comma - precedence 1
          CreateTestCase(
            new[]
            {
              new Token("1", TokenType.Integer), new Token("2", TokenType.Integer), new Token(",", TokenType.Comma)
            },
            Create("1", TokenType.Integer, ElementType.Integer),
            Create("2", TokenType.Integer, ElementType.Integer),
            Create(",", TokenType.Comma, ElementType.Comma)
          )
        };
      }
    }

    public static IEnumerable<object[]> PrecedenceLevel0Data
    {
      get
      {
        return new[]
        {
          // Semicolon - precedence 0
          CreateTestCase(
            new[]
            {
              new Token("1", TokenType.Integer), new Token("2", TokenType.Integer), new Token(";", TokenType.Semicolon)
            },
            Create("1", TokenType.Integer, ElementType.Integer),
            Create("2", TokenType.Integer, ElementType.Integer),
            Create(";", TokenType.Semicolon, ElementType.Semicolon)
          )
        };
      }
    }

    public static IEnumerable<object[]> MixedPrecedenceData
    {
      get
      {
        return new[]
        {
          // Test mixed precedence: 2 + 3 * 4 (parser should correctly identify operators)
          CreateTestCase(
            new[]
            {
              new Token("2", TokenType.Integer),
              new Token("+", TokenType.PlusSign),
              new Token("3", TokenType.Integer),
              new Token("*", TokenType.MultiplyOperator),
              new Token("4", TokenType.Integer)
            },
            Create("2", TokenType.Integer, ElementType.Integer),
            Create("+", TokenType.PlusSign, ElementType.AddOperator),
            Create("3", TokenType.Integer, ElementType.Integer),
            Create("*", TokenType.MultiplyOperator, ElementType.MultiplyOperator),
            Create("4", TokenType.Integer, ElementType.Integer)
          )
        };
      }
    }

    public static IEnumerable<object[]> AssociativityData
    {
      get
      {
        return new[]
        {
          // Left-to-right associativity: 1 - 2 - 3 (parser should identify both as subtract)
          CreateTestCase(
            new[]
            {
              new Token("1", TokenType.Integer),
              new Token("-", TokenType.MinusSign),
              new Token("2", TokenType.Integer),
              new Token("-", TokenType.MinusSign),
              new Token("3", TokenType.Integer)
            },
            Create("1", TokenType.Integer, ElementType.Integer),
            Create("-", TokenType.MinusSign, ElementType.SubtractOperator),
            Create("2", TokenType.Integer, ElementType.Integer),
            Create("-", TokenType.MinusSign, ElementType.SubtractOperator),
            Create("3", TokenType.Integer, ElementType.Integer)
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