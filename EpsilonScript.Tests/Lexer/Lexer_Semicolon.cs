using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.Lexer
{
  public class Lexer_Semicolon : LexerTestBase
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    internal void Lexer_Semicolon_Correctly(string input, params Token[] expected)
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