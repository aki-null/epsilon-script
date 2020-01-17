using System.Collections.Generic;
using Xunit;

namespace EpsilonScript.Tests
{
  public class Lexer_NumericLiteral : Lexer_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void Lexer_NumericLiteral_Correctly(string input, params Lexer.Token[] expected)
    {
      Succeeds(input, expected);
    }

    [Theory]
    [MemberData(nameof(IncorrectData))]
    public void Lexer_NumericLiteral_Fails(string input)
    {
      Fails(input);
    }

    public static IEnumerable<object[]> CorrectData
    {
      get
      {
        return new[]
        {
          new object[]
          {
            "1",
            new Lexer.Token("1", Lexer.TokenType.Integer)
          },
          new object[]
          {
            "01",
            new Lexer.Token("01", Lexer.TokenType.Integer)
          },
          new object[]
          {
            "123456789",
            new Lexer.Token("123456789", Lexer.TokenType.Integer)
          },
          new object[]
          {
            "+123",
            new Lexer.Token("+", Lexer.TokenType.PlusSign),
            new Lexer.Token("123", Lexer.TokenType.Integer)
          },
          new object[]
          {
            "-123",
            new Lexer.Token("-", Lexer.TokenType.MinusSign),
            new Lexer.Token("123", Lexer.TokenType.Integer)
          },
          new object[]
          {
            "1.0",
            new Lexer.Token("1.0", Lexer.TokenType.Float)
          },
          new object[]
          {
            "1.",
            new Lexer.Token("1.", Lexer.TokenType.Float)
          },
          new object[]
          {
            "1234567890.1234567890",
            new Lexer.Token("1234567890.1234567890", Lexer.TokenType.Float)
          },
          new object[]
          {
            "1000.0",
            new Lexer.Token("1000.0", Lexer.TokenType.Float)
          },
          new object[]
          {
            "1.0e0",
            new Lexer.Token("1.0e0", Lexer.TokenType.Float)
          },
          new object[]
          {
            "1.0e99",
            new Lexer.Token("1.0e99", Lexer.TokenType.Float)
          },
          new object[]
          {
            "1.0e+99",
            new Lexer.Token("1.0e+99", Lexer.TokenType.Float)
          },
          new object[]
          {
            "1.0e-0",
            new Lexer.Token("1.0e-0", Lexer.TokenType.Float)
          },
          new object[]
          {
            "1.0e-99",
            new Lexer.Token("1.0e-99", Lexer.TokenType.Float)
          },
        };
      }
    }

    public static IEnumerable<object[]> IncorrectData
    {
      get
      {
        return new[]
        {
          new object[] { "0.0e" },
          new object[] { "100.e" },
          new object[] { "+100.e" },
          new object[] { "-100.e" },
          new object[] { "100.e+" },
          new object[] { "100.e-" },
          new object[] { "+100.e+" },
          new object[] { "+100.e-" },
          new object[] { "-100.e+" },
          new object[] { "-100.e-" },
        };
      }
    }
  }
}
