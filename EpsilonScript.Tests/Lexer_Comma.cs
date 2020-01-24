using System.Collections.Generic;
using EpsilonScript.Lexer;
using Xunit;

namespace EpsilonScript.Tests
{
  public class Lexer_Comma : Lexer_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void Lexer_Comma_Correctly(string input, params Token[] expected)
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