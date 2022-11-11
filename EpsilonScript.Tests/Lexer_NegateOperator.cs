using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;

namespace EpsilonScript.Tests
{
  public class Lexer_NegateOperator : Lexer_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    internal void Lexer_NegateOperator_Correctly(string input, params Token[] expected)
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
            "!",
            new Token("!", TokenType.NegateOperator)
          },
          new object[]
          {
            "!!",
            new Token("!", TokenType.NegateOperator),
            new Token("!", TokenType.NegateOperator)
          },
          new object[]
          {
            "! !",
            new Token("!", TokenType.NegateOperator),
            new Token("!", TokenType.NegateOperator)
          },
          new object[]
          {
            " !",
            new Token("!", TokenType.NegateOperator)
          },
          new object[]
          {
            " ! ",
            new Token("!", TokenType.NegateOperator)
          },
          new object[]
          {
            "! !=",
            new Token("!", TokenType.NegateOperator),
            new Token("!=", TokenType.ComparisonNotEqual),
          },
          new object[]
          {
            "!!=",
            new Token("!", TokenType.NegateOperator),
            new Token("!=", TokenType.ComparisonNotEqual),
          },
        };
      }
    }
  }
}