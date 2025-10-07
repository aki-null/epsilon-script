using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.Lexer
{
  public class Lexer_Identifier : LexerTestBase
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    internal void Lexer_Identifier_Correctly(string input, params Token[] expected)
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
          new object[]
          {
            "variable.name",
            new Token("variable.name", TokenType.Identifier)
          },
          new object[]
          {
            "my.function.call",
            new Token("my.function.call", TokenType.Identifier)
          },
          new object[]
          {
            "obj.prop.value",
            new Token("obj.prop.value", TokenType.Identifier)
          },
          new object[]
          {
            "_internal.helper.func",
            new Token("_internal.helper.func", TokenType.Identifier)
          },
          new object[]
          {
            "test123.var456.item",
            new Token("test123.var456.item", TokenType.Identifier)
          },
        };
      }
    }
  }
}