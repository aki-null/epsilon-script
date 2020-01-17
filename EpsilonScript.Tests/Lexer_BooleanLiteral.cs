using System.Collections.Generic;
using Xunit;

namespace EpsilonScript.Tests
{
  public class Lexer_BooleanLiteral : Lexer_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void Lexer_Identifier_Correctly(string input, params Lexer.Token[] expected)
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
            "true",
            new Lexer.Token("true", Lexer.TokenType.BooleanLiteralTrue)
          },
          new object[]
          {
            "false",
            new Lexer.Token("false", Lexer.TokenType.BooleanLiteralFalse)
          },
          new object[]
          {
            " true",
            new Lexer.Token("true", Lexer.TokenType.BooleanLiteralTrue)
          },
          new object[]
          {
            " false",
            new Lexer.Token("false", Lexer.TokenType.BooleanLiteralFalse)
          },
          new object[]
          {
            "true ",
            new Lexer.Token("true", Lexer.TokenType.BooleanLiteralTrue)
          },
          new object[]
          {
            "false ",
            new Lexer.Token("false", Lexer.TokenType.BooleanLiteralFalse)
          },
          new object[]
          {
            "true false",
            new Lexer.Token("true", Lexer.TokenType.BooleanLiteralTrue),
            new Lexer.Token("false", Lexer.TokenType.BooleanLiteralFalse)
          },
          new object[]
          {
            "false true",
            new Lexer.Token("false", Lexer.TokenType.BooleanLiteralFalse),
            new Lexer.Token("true", Lexer.TokenType.BooleanLiteralTrue)
          },
        };
      }
    }
  }
}
