using System.Collections.Generic;
using EpsilonScript.Lexer;
using Xunit;

namespace EpsilonScript.Tests
{
  public class Lexer_Math : Lexer_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void Lexer_Math_Correctly(string input, params Token[] expected)
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
            "+",
            new Token("+", TokenType.PlusSign)
          },
          new object[]
          {
            " +",
            new Token("+", TokenType.PlusSign)
          },
          new object[]
          {
            "+ ",
            new Token("+", TokenType.PlusSign)
          },
          new object[]
          {
            "+ +",
            new Token("+", TokenType.PlusSign),
            new Token("+", TokenType.PlusSign)
          },
          new object[]
          {
            "++",
            new Token("+", TokenType.PlusSign),
            new Token("+", TokenType.PlusSign)
          },
          new object[]
          {
            "-",
            new Token("-", TokenType.MinusSign)
          },
          new object[]
          {
            " -",
            new Token("-", TokenType.MinusSign)
          },
          new object[]
          {
            "- ",
            new Token("-", TokenType.MinusSign)
          },
          new object[]
          {
            "- -",
            new Token("-", TokenType.MinusSign),
            new Token("-", TokenType.MinusSign)
          },
          new object[]
          {
            "--",
            new Token("-", TokenType.MinusSign),
            new Token("-", TokenType.MinusSign)
          },
          new object[]
          {
            "*",
            new Token("*", TokenType.MultiplyOperator)
          },
          new object[]
          {
            " *",
            new Token("*", TokenType.MultiplyOperator)
          },
          new object[]
          {
            "* ",
            new Token("*", TokenType.MultiplyOperator)
          },
          new object[]
          {
            "* *",
            new Token("*", TokenType.MultiplyOperator),
            new Token("*", TokenType.MultiplyOperator)
          },
          new object[]
          {
            "**",
            new Token("*", TokenType.MultiplyOperator),
            new Token("*", TokenType.MultiplyOperator)
          },
          new object[]
          {
            "/",
            new Token("/", TokenType.DivideOperator)
          },
          new object[]
          {
            " /",
            new Token("/", TokenType.DivideOperator)
          },
          new object[]
          {
            "/ ",
            new Token("/", TokenType.DivideOperator)
          },
          new object[]
          {
            "/ /",
            new Token("/", TokenType.DivideOperator),
            new Token("/", TokenType.DivideOperator)
          },
          new object[]
          {
            "//",
            new Token("/", TokenType.DivideOperator),
            new Token("/", TokenType.DivideOperator)
          },
        };
      }
    }
  }
}