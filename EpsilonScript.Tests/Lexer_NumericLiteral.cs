using System.Collections.Generic;
using EpsilonScript.Lexer;
using Xunit;

namespace EpsilonScript.Tests
{
  public class Lexer_NumericLiteral : Lexer_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void Lexer_NumericLiteral_Correctly(string input, params Token[] expected)
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
            new Token("1", TokenType.Integer)
          },
          new object[]
          {
            "01",
            new Token("01", TokenType.Integer)
          },
          new object[]
          {
            "123456789",
            new Token("123456789", TokenType.Integer)
          },
          new object[]
          {
            "+123",
            new Token("+", TokenType.PlusSign),
            new Token("123", TokenType.Integer)
          },
          new object[]
          {
            "-123",
            new Token("-", TokenType.MinusSign),
            new Token("123", TokenType.Integer)
          },
          new object[]
          {
            "1.0",
            new Token("1.0", TokenType.Float)
          },
          new object[]
          {
            "1.",
            new Token("1.", TokenType.Float)
          },
          new object[]
          {
            "1234567890.1234567890",
            new Token("1234567890.1234567890", TokenType.Float)
          },
          new object[]
          {
            "1000.0",
            new Token("1000.0", TokenType.Float)
          },
          new object[]
          {
            "1.0e0",
            new Token("1.0e0", TokenType.Float)
          },
          new object[]
          {
            "1.0e99",
            new Token("1.0e99", TokenType.Float)
          },
          new object[]
          {
            "1.0e+99",
            new Token("1.0e+99", TokenType.Float)
          },
          new object[]
          {
            "1.0e-0",
            new Token("1.0e-0", TokenType.Float)
          },
          new object[]
          {
            "1.0e-99",
            new Token("1.0e-99", TokenType.Float)
          },
          new object[]
          {
            "1.0E0",
            new Token("1.0E0", TokenType.Float)
          },
          new object[]
          {
            "1.0E99",
            new Token("1.0E99", TokenType.Float)
          },
          new object[]
          {
            "1.0E+99",
            new Token("1.0E+99", TokenType.Float)
          },
          new object[]
          {
            "1.0E-0",
            new Token("1.0E-0", TokenType.Float)
          },
          new object[]
          {
            "1.0E-99",
            new Token("1.0E-99", TokenType.Float)
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
          new object[] {"0.0e"},
          new object[] {"100.e"},
          new object[] {"+100.e"},
          new object[] {"-100.e"},
          new object[] {"100.e+"},
          new object[] {"100.e-"},
          new object[] {"+100.e+"},
          new object[] {"+100.e-"},
          new object[] {"-100.e+"},
          new object[] {"-100.e-"},
        };
      }
    }
  }
}