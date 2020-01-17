using System.Collections.Generic;
using Xunit;

namespace EpsilonScript.Tests
{
  public class Lexer_NegateOperator : Lexer_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void Lexer_NegateOperator_Correctly(string input, params Lexer.Token[] expected)
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
            new Lexer.Token("!", Lexer.TokenType.NegateOperator)
          },
          new object[]
          {
            "!!",
            new Lexer.Token("!", Lexer.TokenType.NegateOperator),
            new Lexer.Token("!", Lexer.TokenType.NegateOperator)
          },
          new object[]
          {
            "! !",
            new Lexer.Token("!", Lexer.TokenType.NegateOperator),
            new Lexer.Token("!", Lexer.TokenType.NegateOperator)
          },
          new object[]
          {
            " !",
            new Lexer.Token("!", Lexer.TokenType.NegateOperator)
          },
          new object[]
          {
            " ! ",
            new Lexer.Token("!", Lexer.TokenType.NegateOperator)
          },
          new object[]
          {
            "! !=",
            new Lexer.Token("!", Lexer.TokenType.NegateOperator),
            new Lexer.Token("!=", Lexer.TokenType.ComparisonNotEqual),
          },
          new object[]
          {
            "!!=",
            new Lexer.Token("!", Lexer.TokenType.NegateOperator),
            new Lexer.Token("!=", Lexer.TokenType.ComparisonNotEqual),
          },
        };
      }
    }
  }
}
