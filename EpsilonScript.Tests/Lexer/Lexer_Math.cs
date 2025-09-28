using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.Lexer
{
  [Trait("Category", "Unit")]
  [Trait("Component", "Lexer")]
  public class Lexer_Math : LexerTestBase
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void Lexer_Math_Correctly(string input, params Token[] expected)
    {
      AssertLexSucceeds(input, expected);
    }

    public static IEnumerable<object[]> CorrectData
    {
      get
      {
        return new[]
        {
          // Individual math operators
          new object[]
          {
            "+",
            new Token("+", TokenType.PlusSign)
          },
          new object[]
          {
            "-",
            new Token("-", TokenType.MinusSign)
          },
          new object[]
          {
            "*",
            new Token("*", TokenType.MultiplyOperator)
          },
          new object[]
          {
            "/",
            new Token("/", TokenType.DivideOperator)
          },
          new object[]
          {
            "%",
            new Token("%", TokenType.ModuloOperator)
          },
          // Multiple operators
          new object[]
          {
            "+-*/",
            new Token("+", TokenType.PlusSign),
            new Token("-", TokenType.MinusSign),
            new Token("*", TokenType.MultiplyOperator),
            new Token("/", TokenType.DivideOperator)
          },
          new object[]
          {
            "+ - * /",
            new Token("+", TokenType.PlusSign),
            new Token("-", TokenType.MinusSign),
            new Token("*", TokenType.MultiplyOperator),
            new Token("/", TokenType.DivideOperator)
          }
        };
      }
    }
  }
}