using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;
using EpsilonScript.Tests.TestInfrastructure.Fakes;

namespace EpsilonScript.Tests.Lexer
{
  public class Lexer_Comma : LexerTestBase
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void Lexer_Comma_Correctly(string input, params Token[] expected)
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