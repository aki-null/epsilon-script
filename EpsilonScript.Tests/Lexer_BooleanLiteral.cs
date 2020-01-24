using System.Collections.Generic;
using EpsilonScript.Lexer;
using Xunit;

namespace EpsilonScript.Tests
{
  public class Lexer_BooleanLiteral : Lexer_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void Lexer_Identifier_Correctly(string input, params Token[] expected)
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