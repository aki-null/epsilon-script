using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.Lexer
{
  public class LexerSemicolonTests : LexerTestBase
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    internal void Lexer_SemicolonTokens_TokenizeCorrectly(string input, params Token[] expected)
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