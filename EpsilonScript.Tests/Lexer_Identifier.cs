using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;

namespace EpsilonScript.Tests
{
  public class Lexer_Identifier : Lexer_Base
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
            "a",
            new Token("a", TokenType.Identifier)
          },
          new object[]
          {
            "hello",
            new Token("hello", TokenType.Identifier)
          },
          new object[]
          {
            "hello world",
            new Token("hello", TokenType.Identifier),
            new Token("world", TokenType.Identifier)
          },
          new object[]
          {
            "hello    world",
            new Token("hello", TokenType.Identifier),
            new Token("world", TokenType.Identifier)
          },
          new object[]
          {
            "hello ",
            new Token("hello", TokenType.Identifier)
          },
          new object[]
          {
            " hello",
            new Token("hello", TokenType.Identifier)
          },
          new object[]
          {
            "_hello",
            new Token("_hello", TokenType.Identifier)
          },
          new object[]
          {
            "_hello10",
            new Token("_hello10", TokenType.Identifier)
          },
        };
      }
    }
  }
}