using System.Collections.Generic;
using Xunit;

namespace EpsilonScript.Tests
{
  public class Lexer_Math : Lexer_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void Lexer_Math_Correctly(string input, params Lexer.Token[] expected)
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
            new Lexer.Token("+", Lexer.TokenType.PlusSign)
          },
          new object[]
          {
            " +",
            new Lexer.Token("+", Lexer.TokenType.PlusSign)
          },
          new object[]
          {
            "+ ",
            new Lexer.Token("+", Lexer.TokenType.PlusSign)
          },
          new object[]
          {
            "+ +",
            new Lexer.Token("+", Lexer.TokenType.PlusSign),
            new Lexer.Token("+", Lexer.TokenType.PlusSign)
          },
          new object[]
          {
            "++",
            new Lexer.Token("+", Lexer.TokenType.PlusSign),
            new Lexer.Token("+", Lexer.TokenType.PlusSign)
          },
          new object[]
          {
            "-",
            new Lexer.Token("-", Lexer.TokenType.MinusSign)
          },
          new object[]
          {
            " -",
            new Lexer.Token("-", Lexer.TokenType.MinusSign)
          },
          new object[]
          {
            "- ",
            new Lexer.Token("-", Lexer.TokenType.MinusSign)
          },
          new object[]
          {
            "- -",
            new Lexer.Token("-", Lexer.TokenType.MinusSign),
            new Lexer.Token("-", Lexer.TokenType.MinusSign)
          },
          new object[]
          {
            "--",
            new Lexer.Token("-", Lexer.TokenType.MinusSign),
            new Lexer.Token("-", Lexer.TokenType.MinusSign)
          },
          new object[]
          {
            "*",
            new Lexer.Token("*", Lexer.TokenType.MultiplyOperator)
          },
          new object[]
          {
            " *",
            new Lexer.Token("*", Lexer.TokenType.MultiplyOperator)
          },
          new object[]
          {
            "* ",
            new Lexer.Token("*", Lexer.TokenType.MultiplyOperator)
          },
          new object[]
          {
            "* *",
            new Lexer.Token("*", Lexer.TokenType.MultiplyOperator),
            new Lexer.Token("*", Lexer.TokenType.MultiplyOperator)
          },
          new object[]
          {
            "**",
            new Lexer.Token("*", Lexer.TokenType.MultiplyOperator),
            new Lexer.Token("*", Lexer.TokenType.MultiplyOperator)
          },
          new object[]
          {
            "/",
            new Lexer.Token("/", Lexer.TokenType.DivideOperator)
          },
          new object[]
          {
            " /",
            new Lexer.Token("/", Lexer.TokenType.DivideOperator)
          },
          new object[]
          {
            "/ ",
            new Lexer.Token("/", Lexer.TokenType.DivideOperator)
          },
          new object[]
          {
            "/ /",
            new Lexer.Token("/", Lexer.TokenType.DivideOperator),
            new Lexer.Token("/", Lexer.TokenType.DivideOperator)
          },
          new object[]
          {
            "//",
            new Lexer.Token("/", Lexer.TokenType.DivideOperator),
            new Lexer.Token("/", Lexer.TokenType.DivideOperator)
          },
        };
      }
    }
  }
}
