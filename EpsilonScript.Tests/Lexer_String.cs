using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;

namespace EpsilonScript.Tests
{
  public class Lexer_String : Lexer_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    internal void Lexer_String_Correctly(string input, params Token[] expected)
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
            "\"\"",
            new Token("\"\"", TokenType.String)
          },
          new object[]
          {
            "\"Hello World\"",
            new Token("\"Hello World\"", TokenType.String)
          },
          new object[]
          {
            " \"Hello World\"",
            new Token("\"Hello World\"", TokenType.String)
          },
          new object[]
          {
            "\"Hello World\" ",
            new Token("\"Hello World\"", TokenType.String)
          },
          new object[]
          {
            "\"こんにちは世界\"",
            new Token("\"こんにちは世界\"", TokenType.String)
          },
          new object[]
          {
            "\"Hello World\"\"こんにちは世界\"",
            new Token("\"Hello World\"", TokenType.String),
            new Token("\"こんにちは世界\"", TokenType.String)
          },
          new object[]
          {
            "\"Hello World\" \"こんにちは世界\"",
            new Token("\"Hello World\"", TokenType.String),
            new Token("\"こんにちは世界\"", TokenType.String)
          },
        };
      }
    }
  }
}