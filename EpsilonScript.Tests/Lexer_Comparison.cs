using System.Collections.Generic;
using Xunit;

namespace EpsilonScript.Tests
{
  public class Lexer_Comparison : Lexer_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void Lexer_Comparison_Correctly(string input, params Lexer.Token[] expected)
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
            new Lexer.Token("==", Lexer.TokenType.ComparisonEqual)
          },
          new object[]
          {
            "== ==",
            new Lexer.Token("==", Lexer.TokenType.ComparisonEqual),
            new Lexer.Token("==", Lexer.TokenType.ComparisonEqual)
          },
          new object[]
          {
            "====",
            new Lexer.Token("==", Lexer.TokenType.ComparisonEqual),
            new Lexer.Token("==", Lexer.TokenType.ComparisonEqual)
          },
          new object[]
          {
            "!=",
            new Lexer.Token("!=", Lexer.TokenType.ComparisonNotEqual)
          },
          new object[]
          {
            "!= !=",
            new Lexer.Token("!=", Lexer.TokenType.ComparisonNotEqual),
            new Lexer.Token("!=", Lexer.TokenType.ComparisonNotEqual)
          },
          new object[]
          {
            "!=!=",
            new Lexer.Token("!=", Lexer.TokenType.ComparisonNotEqual),
            new Lexer.Token("!=", Lexer.TokenType.ComparisonNotEqual)
          },
          new object[]
          {
            "<=",
            new Lexer.Token("<=", Lexer.TokenType.ComparisonLessThanOrEqualTo)
          },
          new object[]
          {
            "<= <=",
            new Lexer.Token("<=", Lexer.TokenType.ComparisonLessThanOrEqualTo),
            new Lexer.Token("<=", Lexer.TokenType.ComparisonLessThanOrEqualTo)
          },
          new object[]
          {
            "<=<=",
            new Lexer.Token("<=", Lexer.TokenType.ComparisonLessThanOrEqualTo),
            new Lexer.Token("<=", Lexer.TokenType.ComparisonLessThanOrEqualTo)
          },
          new object[]
          {
            ">=",
            new Lexer.Token(">=", Lexer.TokenType.ComparisonGreaterThanOrEqualTo)
          },
          new object[]
          {
            ">= >=",
            new Lexer.Token(">=", Lexer.TokenType.ComparisonGreaterThanOrEqualTo),
            new Lexer.Token(">=", Lexer.TokenType.ComparisonGreaterThanOrEqualTo)
          },
          new object[]
          {
            ">=>=",
            new Lexer.Token(">=", Lexer.TokenType.ComparisonGreaterThanOrEqualTo),
            new Lexer.Token(">=", Lexer.TokenType.ComparisonGreaterThanOrEqualTo)
          },
          new object[]
          {
            "<",
            new Lexer.Token("<", Lexer.TokenType.ComparisonLessThan)
          },
          new object[]
          {
            "< <",
            new Lexer.Token("<", Lexer.TokenType.ComparisonLessThan),
            new Lexer.Token("<", Lexer.TokenType.ComparisonLessThan)
          },
          new object[]
          {
            "<<",
            new Lexer.Token("<", Lexer.TokenType.ComparisonLessThan),
            new Lexer.Token("<", Lexer.TokenType.ComparisonLessThan)
          },
          new object[]
          {
            ">",
            new Lexer.Token(">", Lexer.TokenType.ComparisonGreaterThan)
          },
          new object[]
          {
            "> >",
            new Lexer.Token(">", Lexer.TokenType.ComparisonGreaterThan),
            new Lexer.Token(">", Lexer.TokenType.ComparisonGreaterThan)
          },
          new object[]
          {
            ">>",
            new Lexer.Token(">", Lexer.TokenType.ComparisonGreaterThan),
            new Lexer.Token(">", Lexer.TokenType.ComparisonGreaterThan)
          },
        };
      }
    }
  }
}
