using System.Collections.Generic;
using Xunit;

namespace EpsilonScript.Tests
{
  public class Lexer_Comma : Lexer_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void Lexer_Comma_Correctly(string input, params Lexer.Token[] expected)
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
            ",",
            new Lexer.Token(",", Lexer.TokenType.Comma)
          },
          new object[]
          {
            " ,",
            new Lexer.Token(",", Lexer.TokenType.Comma)
          },
          new object[]
          {
            ", ",
            new Lexer.Token(",", Lexer.TokenType.Comma)
          },
          new object[]
          {
            ",,",
            new Lexer.Token(",", Lexer.TokenType.Comma),
            new Lexer.Token(",", Lexer.TokenType.Comma)
          },
          new object[]
          {
            ", ,",
            new Lexer.Token(",", Lexer.TokenType.Comma),
            new Lexer.Token(",", Lexer.TokenType.Comma)
          },
          new object[]
          {
            ",,,",
            new Lexer.Token(",", Lexer.TokenType.Comma),
            new Lexer.Token(",", Lexer.TokenType.Comma),
            new Lexer.Token(",", Lexer.TokenType.Comma)
          },
        };
      }
    }
  }
}
