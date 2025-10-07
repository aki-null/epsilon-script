using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.Lexer
{
  [Trait("Category", "Unit")]
  [Trait("Component", "Lexer")]
  public class Lexer_BooleanLiteral : LexerTestBase
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    internal void Lexer_BooleanLiteral_Correctly(string input, params Token[] expected)
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
            "true",
            new Token("true", TokenType.BooleanLiteralTrue)
          },
          new object[]
          {
            "false",
            new Token("false", TokenType.BooleanLiteralFalse)
          },
          new object[]
          {
            " true",
            new Token("true", TokenType.BooleanLiteralTrue)
          },
          new object[]
          {
            " false",
            new Token("false", TokenType.BooleanLiteralFalse)
          },
          new object[]
          {
            "true ",
            new Token("true", TokenType.BooleanLiteralTrue)
          },
          new object[]
          {
            "false ",
            new Token("false", TokenType.BooleanLiteralFalse)
          },
          new object[]
          {
            "true false",
            new Token("true", TokenType.BooleanLiteralTrue),
            new Token("false", TokenType.BooleanLiteralFalse)
          },
          new object[]
          {
            "false true",
            new Token("false", TokenType.BooleanLiteralFalse),
            new Token("true", TokenType.BooleanLiteralTrue)
          },
        };
      }
    }
  }
}