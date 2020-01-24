using System.Collections.Generic;
using EpsilonScript.Lexer;
using Xunit;

namespace EpsilonScript.Tests
{
  public class Lexer_Semicolon : Lexer_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void Lexer_Semicolon_Correctly(string input, params Token[] expected)
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
            new Token(";", TokenType.Semicolon)
          },
          new object[]
          {
            " ;",
            new Token(";", TokenType.Semicolon)
          },
          new object[]
          {
            "; ",
            new Token(";", TokenType.Semicolon)
          },
          new object[]
          {
            "; ;",
            new Token(";", TokenType.Semicolon),
            new Token(";", TokenType.Semicolon)
          },
          new object[]
          {
            ";;",
            new Token(";", TokenType.Semicolon),
            new Token(";", TokenType.Semicolon)
          },
        };
      }
    }
  }
}