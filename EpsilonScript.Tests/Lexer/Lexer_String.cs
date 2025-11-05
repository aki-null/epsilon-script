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
    internal void Lexer_String_Correctly(string input, params Token[] expected)
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
          // Single quote tests
          new object[]
          {
            "''",
            new Token("''", TokenType.String)
          },
          new object[]
          {
            "'Hello World'",
            new Token("'Hello World'", TokenType.String)
          },
          new object[]
          {
            "'こんにちは世界'",
            new Token("'こんにちは世界'", TokenType.String)
          },
          new object[]
          {
            "'Hello' 'World'",
            new Token("'Hello'", TokenType.String),
            new Token("'World'", TokenType.String)
          },
          // Single quotes with double quotes inside
          new object[]
          {
            "'He said \"hello\"'",
            new Token("'He said \"hello\"'", TokenType.String)
          },
          // Double quotes with single quotes inside
          new object[]
          {
            "\"It's working\"",
            new Token("\"It's working\"", TokenType.String)
          },
          // Backslashes are literal (no escape sequences)
          new object[]
          {
            "\"C:\\\\Users\\\\Name\"",
            new Token("\"C:\\\\Users\\\\Name\"", TokenType.String)
          },
          new object[]
          {
            "'path\\to\\file'",
            new Token("'path\\to\\file'", TokenType.String)
          },
        };
      }
    }
  }
}