using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;
using static EpsilonScript.Tests.TestInfrastructure.ElementFactory;

namespace EpsilonScript.Tests.Parser
{
  /// <summary>
  /// Tests parser precedence levels by verifying token-to-element conversion.
  ///
  /// IMPORTANT: All test data uses INFIX notation (e.g., [2, *, 3] not [2, 3, *]).
  /// The parser preserves infix order - it does NOT convert to postfix/RPN.
  /// See TokenParser.cs class documentation for architectural details.
  /// </summary>
  public class ParserPrecedenceTests : TokenParserTestBase
  {
    [Theory]
    [MemberData(nameof(PrecedenceLevel8Data))]
    internal void PrecedenceLevel8_FunctionAndUnaryOperators(Token[] input, Element[] expected)
    {
      AssertParseSucceeds(input, expected);
    }

    [Theory]
    [MemberData(nameof(PrecedenceLevel7Data))]
    internal void PrecedenceLevel7_MultiplicationDivisionModulo(Token[] input, Element[] expected)
    {
      AssertParseSucceeds(input, expected);
    }

    [Theory]
    [MemberData(nameof(PrecedenceLevel6Data))]
    internal void PrecedenceLevel6_AdditionSubtraction(Token[] input, Element[] expected)
    {
      AssertParseSucceeds(input, expected);
    }

    [Theory]
    [MemberData(nameof(PrecedenceLevel5Data))]
    internal void PrecedenceLevel5_ComparisonOperators(Token[] input, Element[] expected)
    {
      AssertParseSucceeds(input, expected);
    }

    [Theory]
    [MemberData(nameof(PrecedenceLevel4Data))]
    internal void PrecedenceLevel4_BooleanAnd(Token[] input, Element[] expected)
    {
      AssertParseSucceeds(input, expected);
    }

    [Theory]
    [MemberData(nameof(PrecedenceLevel3Data))]
    internal void PrecedenceLevel3_BooleanOr(Token[] input, Element[] expected)
    {
      AssertParseSucceeds(input, expected);
    }

    [Theory]
    [MemberData(nameof(PrecedenceLevel2Data))]
    internal void PrecedenceLevel2_AssignmentOperators(Token[] input, Element[] expected)
    {
      AssertParseSucceeds(input, expected);
    }

    [Theory]
    [MemberData(nameof(PrecedenceLevel1Data))]
    internal void PrecedenceLevel1_Comma(Token[] input, Element[] expected)
    {
      AssertParseSucceeds(input, expected);
    }

    [Theory]
    [MemberData(nameof(PrecedenceLevel0Data))]
    internal void PrecedenceLevel0_Semicolon(Token[] input, Element[] expected)
    {
      AssertParseSucceeds(input, expected);
    }

    [Theory]
    [MemberData(nameof(MixedPrecedenceData))]
    internal void Parser_MixedPrecedenceLevels_CorrectOrderOfOperations(Token[] input, Element[] expected)
    {
      AssertParseSucceeds(input, expected);
    }

    [Theory]
    [MemberData(nameof(AssociativityData))]
    internal void Parser_Associativity_CorrectEvaluation(Token[] input, Element[] expected)
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
              new Token("2", TokenType.Integer), new Token("*", TokenType.MultiplyOperator),
              new Token("3", TokenType.Integer)
            },
            Create("2", TokenType.Integer, ElementType.Integer),
            Create("*", TokenType.MultiplyOperator, ElementType.MultiplyOperator),
            Create("3", TokenType.Integer, ElementType.Integer)
          ),
          CreateTestCase(
            new[]
            {
              new Token("8", TokenType.Integer), new Token("/", TokenType.DivideOperator),
              new Token("2", TokenType.Integer)
            },
            Create("8", TokenType.Integer, ElementType.Integer),
            Create("/", TokenType.DivideOperator, ElementType.DivideOperator),
            Create("2", TokenType.Integer, ElementType.Integer)
          ),
          CreateTestCase(
            new[]
            {
              new Token("7", TokenType.Integer), new Token("%", TokenType.ModuloOperator),
              new Token("3", TokenType.Integer)
            },
            Create("7", TokenType.Integer, ElementType.Integer),
            Create("%", TokenType.ModuloOperator, ElementType.ModuloOperator),
            Create("3", TokenType.Integer, ElementType.Integer)
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
              new Token("1", TokenType.Integer), new Token("+", TokenType.PlusSign), new Token("2", TokenType.Integer)
            },
            Create("1", TokenType.Integer, ElementType.Integer),
            Create("+", TokenType.PlusSign, ElementType.AddOperator),
            Create("2", TokenType.Integer, ElementType.Integer)
          ),
          CreateTestCase(
            new[]
            {
              new Token("5", TokenType.Integer), new Token("-", TokenType.MinusSign), new Token("3", TokenType.Integer)
            },
            Create("5", TokenType.Integer, ElementType.Integer),
            Create("-", TokenType.MinusSign, ElementType.SubtractOperator),
            Create("3", TokenType.Integer, ElementType.Integer)
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
              new Token("1", TokenType.Integer), new Token("==", TokenType.ComparisonEqual),
              new Token("2", TokenType.Integer)
            },
            Create("1", TokenType.Integer, ElementType.Integer),
            Create("==", TokenType.ComparisonEqual, ElementType.ComparisonEqual),
            Create("2", TokenType.Integer, ElementType.Integer)
          ),
          CreateTestCase(
            new[]
            {
              new Token("1", TokenType.Integer), new Token("!=", TokenType.ComparisonNotEqual),
              new Token("2", TokenType.Integer)
            },
            Create("1", TokenType.Integer, ElementType.Integer),
            Create("!=", TokenType.ComparisonNotEqual, ElementType.ComparisonNotEqual),
            Create("2", TokenType.Integer, ElementType.Integer)
          ),
          CreateTestCase(
            new[]
            {
              new Token("1", TokenType.Integer), new Token("<", TokenType.ComparisonLessThan),
              new Token("2", TokenType.Integer)
            },
            Create("1", TokenType.Integer, ElementType.Integer),
            Create("<", TokenType.ComparisonLessThan, ElementType.ComparisonLessThan),
            Create("2", TokenType.Integer, ElementType.Integer)
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
              new Token("true", TokenType.BooleanLiteralTrue), new Token("&&", TokenType.BooleanAndOperator),
              new Token("false", TokenType.BooleanLiteralFalse)
            },
            Create("true", TokenType.BooleanLiteralTrue, ElementType.BooleanLiteralTrue),
            Create("&&", TokenType.BooleanAndOperator, ElementType.BooleanAndOperator),
            Create("false", TokenType.BooleanLiteralFalse, ElementType.BooleanLiteralFalse)
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
              new Token("true", TokenType.BooleanLiteralTrue), new Token("||", TokenType.BooleanOrOperator),
              new Token("false", TokenType.BooleanLiteralFalse)
            },
            Create("true", TokenType.BooleanLiteralTrue, ElementType.BooleanLiteralTrue),
            Create("||", TokenType.BooleanOrOperator, ElementType.BooleanOrOperator),
            Create("false", TokenType.BooleanLiteralFalse, ElementType.BooleanLiteralFalse)
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
              new Token("x", TokenType.Identifier), new Token("=", TokenType.AssignmentOperator),
              new Token("1", TokenType.Integer)
            },
            Create("x", TokenType.Identifier, ElementType.Variable),
            Create("=", TokenType.AssignmentOperator, ElementType.AssignmentOperator),
            Create("1", TokenType.Integer, ElementType.Integer)
          ),
          CreateTestCase(
            new[]
            {
              new Token("x", TokenType.Identifier), new Token("+=", TokenType.AssignmentAddOperator),
              new Token("1", TokenType.Integer)
            },
            Create("x", TokenType.Identifier, ElementType.Variable),
            Create("+=", TokenType.AssignmentAddOperator, ElementType.AssignmentAddOperator),
            Create("1", TokenType.Integer, ElementType.Integer)
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
              new Token("func", TokenType.Identifier), new Token("(", TokenType.LeftParenthesis),
              new Token("1", TokenType.Integer), new Token(",", TokenType.Comma), new Token("2", TokenType.Integer),
              new Token(")", TokenType.RightParenthesis)
            },
            Create("func", TokenType.Identifier, ElementType.Function),
            Create("(", TokenType.LeftParenthesis, ElementType.FunctionStartParenthesis),
            Create("1", TokenType.Integer, ElementType.Integer),
            Create(",", TokenType.Comma, ElementType.Comma),
            Create("2", TokenType.Integer, ElementType.Integer),
            Create(")", TokenType.RightParenthesis, ElementType.RightParenthesis)
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
              new Token("1", TokenType.Integer), new Token(";", TokenType.Semicolon), new Token("2", TokenType.Integer)
            },
            Create("1", TokenType.Integer, ElementType.Integer),
            Create(";", TokenType.Semicolon, ElementType.Semicolon),
            Create("2", TokenType.Integer, ElementType.Integer)
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