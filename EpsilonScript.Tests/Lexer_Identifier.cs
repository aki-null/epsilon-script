using System.Collections.Generic;
using Xunit;

namespace EpsilonScript.Tests
{
  public class Lexer_Identifier : Lexer_Base
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
            "a",
            new Lexer.Token("a", Lexer.TokenType.Identifier)
          },
          new object[]
          {
            "hello",
            new Lexer.Token("hello", Lexer.TokenType.Identifier)
          },
          new object[]
          {
            "hello world",
            new Lexer.Token("hello", Lexer.TokenType.Identifier),
            new Lexer.Token("world", Lexer.TokenType.Identifier)
          },
          new object[]
          {
            "hello    world",
            new Lexer.Token("hello", Lexer.TokenType.Identifier),
            new Lexer.Token("world", Lexer.TokenType.Identifier)
          },
          new object[]
          {
            "hello ",
            new Lexer.Token("hello", Lexer.TokenType.Identifier)
          },
          new object[]
          {
            " hello",
            new Lexer.Token("hello", Lexer.TokenType.Identifier)
          },
          new object[]
          {
            "_hello",
            new Lexer.Token("_hello", Lexer.TokenType.Identifier)
          },
          new object[]
          {
            "_hello10",
            new Lexer.Token("_hello10", Lexer.TokenType.Identifier)
          },
        };
      }
    }
  }
}
