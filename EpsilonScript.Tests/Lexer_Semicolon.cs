using System.Collections.Generic;
using Xunit;

namespace EpsilonScript.Tests
{
  public class Lexer_Semicolon : Lexer_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void Lexer_Semicolon_Correctly(string input, params Lexer.Token[] expected)
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
            ";",
            new Lexer.Token(";", Lexer.TokenType.Semicolon)
          },
          new object[]
          {
            " ;",
            new Lexer.Token(";", Lexer.TokenType.Semicolon)
          },
          new object[]
          {
            "; ",
            new Lexer.Token(";", Lexer.TokenType.Semicolon)
          },
          new object[]
          {
            "; ;",
            new Lexer.Token(";", Lexer.TokenType.Semicolon),
            new Lexer.Token(";", Lexer.TokenType.Semicolon)
          },
          new object[]
          {
            ";;",
            new Lexer.Token(";", Lexer.TokenType.Semicolon),
            new Lexer.Token(";", Lexer.TokenType.Semicolon)
          },
        };
      }
    }
  }
}
