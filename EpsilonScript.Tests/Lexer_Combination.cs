using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;

namespace EpsilonScript.Tests
{
  public class Lexer_Combination : Lexer_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void Lexer_Combination_Correctly(string input, params Token[] expected)
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
            "(failed == 100 || invincible == true) && !(time >= 25.0e0)",
            new Token("(", TokenType.LeftParenthesis),
            new Token("failed", TokenType.Identifier),
            new Token("==", TokenType.ComparisonEqual),
            new Token("100", TokenType.Integer),
            new Token("||", TokenType.BooleanOrOperator),
            new Token("invincible", TokenType.Identifier),
            new Token("==", TokenType.ComparisonEqual),
            new Token("true", TokenType.BooleanLiteralTrue),
            new Token(")", TokenType.RightParenthesis),
            new Token("&&", TokenType.BooleanAndOperator),
            new Token("!", TokenType.NegateOperator),
            new Token("(", TokenType.LeftParenthesis),
            new Token("time", TokenType.Identifier),
            new Token(">=", TokenType.ComparisonGreaterThanOrEqualTo),
            new Token("25.0e0", TokenType.Float),
            new Token(")", TokenType.RightParenthesis)
          },
          new object[]
          {
            "(failed==100||invincible==true)&&!(time>=25.0e0)",
            new Token("(", TokenType.LeftParenthesis),
            new Token("failed", TokenType.Identifier),
            new Token("==", TokenType.ComparisonEqual),
            new Token("100", TokenType.Integer),
            new Token("||", TokenType.BooleanOrOperator),
            new Token("invincible", TokenType.Identifier),
            new Token("==", TokenType.ComparisonEqual),
            new Token("true", TokenType.BooleanLiteralTrue),
            new Token(")", TokenType.RightParenthesis),
            new Token("&&", TokenType.BooleanAndOperator),
            new Token("!", TokenType.NegateOperator),
            new Token("(", TokenType.LeftParenthesis),
            new Token("time", TokenType.Identifier),
            new Token(">=", TokenType.ComparisonGreaterThanOrEqualTo),
            new Token("25.0e0", TokenType.Float),
            new Token(")", TokenType.RightParenthesis)
          },
          new object[]
          {
            "failed = (count + 1) * 5 / 2; failed > 10",
            new Token("failed", TokenType.Identifier),
            new Token("=", TokenType.AssignmentOperator),
            new Token("(", TokenType.LeftParenthesis),
            new Token("count", TokenType.Identifier),
            new Token("+", TokenType.PlusSign),
            new Token("1", TokenType.Integer),
            new Token(")", TokenType.RightParenthesis),
            new Token("*", TokenType.MultiplyOperator),
            new Token("5", TokenType.Integer),
            new Token("/", TokenType.DivideOperator),
            new Token("2", TokenType.Integer),
            new Token(";", TokenType.Semicolon),
            new Token("failed", TokenType.Identifier),
            new Token(">", TokenType.ComparisonGreaterThan),
            new Token("10", TokenType.Integer)
          },
          new object[]
          {
            "result = rand(0, 10)",
            new Token("result", TokenType.Identifier),
            new Token("=", TokenType.AssignmentOperator),
            new Token("rand", TokenType.Identifier),
            new Token("(", TokenType.LeftParenthesis),
            new Token("0", TokenType.Integer),
            new Token(",", TokenType.Comma),
            new Token("10", TokenType.Integer),
            new Token(")", TokenType.RightParenthesis)
          },
        };
      }
    }
  }
}