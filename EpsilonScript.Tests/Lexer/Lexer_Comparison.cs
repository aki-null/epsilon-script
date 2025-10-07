using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.Lexer
{
  public class Lexer_Comparison : LexerTestBase
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    internal void Lexer_Comparison_Correctly(string input, params Token[] expected)
    {
      AssertLexSucceeds(input, expected);
    }

    public static IEnumerable<object[]> CorrectData
    {
      get
      {
        return new[]
        {
          // Individual comparison operators
          new object[]
          {
            "==",
            new Token("==", TokenType.ComparisonEqual)
          },
          new object[]
          {
            "!=",
            new Token("!=", TokenType.ComparisonNotEqual)
          },
          new object[]
          {
            "<=",
            new Token("<=", TokenType.ComparisonLessThanOrEqualTo)
          },
          new object[]
          {
            ">=",
            new Token(">=", TokenType.ComparisonGreaterThanOrEqualTo)
          },
          new object[]
          {
            "<",
            new Token("<", TokenType.ComparisonLessThan)
          },
          new object[]
          {
            ">",
            new Token(">", TokenType.ComparisonGreaterThan)
          },
          // Multiple comparison operators
          new object[]
          {
            "< <= == >= >",
            new Token("<", TokenType.ComparisonLessThan),
            new Token("<=", TokenType.ComparisonLessThanOrEqualTo),
            new Token("==", TokenType.ComparisonEqual),
            new Token(">=", TokenType.ComparisonGreaterThanOrEqualTo),
            new Token(">", TokenType.ComparisonGreaterThan)
          },
          new object[]
          {
            "!=<=",
            new Token("!=", TokenType.ComparisonNotEqual),
            new Token("<=", TokenType.ComparisonLessThanOrEqualTo)
          }
        };
      }
    }
  }
}