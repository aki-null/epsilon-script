using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.Lexer
{
  [Trait("Category", "Unit")]
  [Trait("Component", "Lexer")]
  public class Lexer_String : LexerTestBase
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void Lexer_String_Correctly(string input, params Token[] expected)
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