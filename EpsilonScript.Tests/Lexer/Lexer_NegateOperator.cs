using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.Lexer
{
  public class Lexer_NegateOperator : LexerTestBase
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    internal void Lexer_NegateOperator_Correctly(string input, params Token[] expected)
    {
      AssertLexSucceeds(input, expected);
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