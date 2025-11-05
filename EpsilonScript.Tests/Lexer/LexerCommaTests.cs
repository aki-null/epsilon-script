using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.Lexer
{
  public class LexerCommaTests : LexerTestBase
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    internal void CommaTokens_TokenizeCorrectly(string input, params Token[] expected)
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
            ",",
            new Token(",", TokenType.Comma)
          },
          new object[]
          {
            " ,",
            new Token(",", TokenType.Comma)
          },
          new object[]
          {
            ", ",
            new Token(",", TokenType.Comma)
          },
          new object[]
          {
            ",,",
            new Token(",", TokenType.Comma),
            new Token(",", TokenType.Comma)
          },
          new object[]
          {
            ", ,",
            new Token(",", TokenType.Comma),
            new Token(",", TokenType.Comma)
          },
          new object[]
          {
            ",,,",
            new Token(",", TokenType.Comma),
            new Token(",", TokenType.Comma),
            new Token(",", TokenType.Comma)
          },
        };
      }
    }
  }
}