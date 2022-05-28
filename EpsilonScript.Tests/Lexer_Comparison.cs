using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;

namespace EpsilonScript.Tests
{
  public class Lexer_Comparison : Lexer_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void Lexer_Comparison_Correctly(string input, params Token[] expected)
    {
      Succeeds(input, expected);
    }

    public static IEnumerable<object[]> CorrectData
    {
      get
      {
        return new[]
        {
          new object[]
          {
            "==",
            new Token("==", TokenType.ComparisonEqual)
          },
          new object[]
          {
            "== ==",
            new Token("==", TokenType.ComparisonEqual),
            new Token("==", TokenType.ComparisonEqual)
          },
          new object[]
          {
            "====",
            new Token("==", TokenType.ComparisonEqual),
            new Token("==", TokenType.ComparisonEqual)
          },
          new object[]
          {
            "!=",
            new Token("!=", TokenType.ComparisonNotEqual)
          },
          new object[]
          {
            "!= !=",
            new Token("!=", TokenType.ComparisonNotEqual),
            new Token("!=", TokenType.ComparisonNotEqual)
          },
          new object[]
          {
            "!=!=",
            new Token("!=", TokenType.ComparisonNotEqual),
            new Token("!=", TokenType.ComparisonNotEqual)
          },
          new object[]
          {
            "<=",
            new Token("<=", TokenType.ComparisonLessThanOrEqualTo)
          },
          new object[]
          {
            "<= <=",
            new Token("<=", TokenType.ComparisonLessThanOrEqualTo),
            new Token("<=", TokenType.ComparisonLessThanOrEqualTo)
          },
          new object[]
          {
            "<=<=",
            new Token("<=", TokenType.ComparisonLessThanOrEqualTo),
            new Token("<=", TokenType.ComparisonLessThanOrEqualTo)
          },
          new object[]
          {
            ">=",
            new Token(">=", TokenType.ComparisonGreaterThanOrEqualTo)
          },
          new object[]
          {
            ">= >=",
            new Token(">=", TokenType.ComparisonGreaterThanOrEqualTo),
            new Token(">=", TokenType.ComparisonGreaterThanOrEqualTo)
          },
          new object[]
          {
            ">=>=",
            new Token(">=", TokenType.ComparisonGreaterThanOrEqualTo),
            new Token(">=", TokenType.ComparisonGreaterThanOrEqualTo)
          },
          new object[]
          {
            "<",
            new Token("<", TokenType.ComparisonLessThan)
          },
          new object[]
          {
            "< <",
            new Token("<", TokenType.ComparisonLessThan),
            new Token("<", TokenType.ComparisonLessThan)
          },
          new object[]
          {
            "<<",
            new Token("<", TokenType.ComparisonLessThan),
            new Token("<", TokenType.ComparisonLessThan)
          },
          new object[]
          {
            ">",
            new Token(">", TokenType.ComparisonGreaterThan)
          },
          new object[]
          {
            "> >",
            new Token(">", TokenType.ComparisonGreaterThan),
            new Token(">", TokenType.ComparisonGreaterThan)
          },
          new object[]
          {
            ">>",
            new Token(">", TokenType.ComparisonGreaterThan),
            new Token(">", TokenType.ComparisonGreaterThan)
          },
        };
      }
    }
  }
}