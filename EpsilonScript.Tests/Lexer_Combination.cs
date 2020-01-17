using System.Collections.Generic;
using Xunit;

namespace EpsilonScript.Tests
{
  public class Lexer_Combination : Lexer_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void Lexer_Combination_Correctly(string input, params Lexer.Token[] expected)
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
            new Lexer.Token("(", Lexer.TokenType.LeftParenthesis),
            new Lexer.Token("failed", Lexer.TokenType.Identifier),
            new Lexer.Token("==", Lexer.TokenType.ComparisonEqual),
            new Lexer.Token("100", Lexer.TokenType.Integer),
            new Lexer.Token("||", Lexer.TokenType.BooleanOrOperator),
            new Lexer.Token("invincible", Lexer.TokenType.Identifier),
            new Lexer.Token("==", Lexer.TokenType.ComparisonEqual),
            new Lexer.Token("true", Lexer.TokenType.BooleanLiteralTrue),
            new Lexer.Token(")", Lexer.TokenType.RightParenthesis),
            new Lexer.Token("&&", Lexer.TokenType.BooleanAndOperator),
            new Lexer.Token("!", Lexer.TokenType.NegateOperator),
            new Lexer.Token("(", Lexer.TokenType.LeftParenthesis),
            new Lexer.Token("time", Lexer.TokenType.Identifier),
            new Lexer.Token(">=", Lexer.TokenType.ComparisonGreaterThanOrEqualTo),
            new Lexer.Token("25.0e0", Lexer.TokenType.Float),
            new Lexer.Token(")", Lexer.TokenType.RightParenthesis)
          },
          new object[]
          {
            "(failed==100||invincible==true)&&!(time>=25.0e0)",
            new Lexer.Token("(", Lexer.TokenType.LeftParenthesis),
            new Lexer.Token("failed", Lexer.TokenType.Identifier),
            new Lexer.Token("==", Lexer.TokenType.ComparisonEqual),
            new Lexer.Token("100", Lexer.TokenType.Integer),
            new Lexer.Token("||", Lexer.TokenType.BooleanOrOperator),
            new Lexer.Token("invincible", Lexer.TokenType.Identifier),
            new Lexer.Token("==", Lexer.TokenType.ComparisonEqual),
            new Lexer.Token("true", Lexer.TokenType.BooleanLiteralTrue),
            new Lexer.Token(")", Lexer.TokenType.RightParenthesis),
            new Lexer.Token("&&", Lexer.TokenType.BooleanAndOperator),
            new Lexer.Token("!", Lexer.TokenType.NegateOperator),
            new Lexer.Token("(", Lexer.TokenType.LeftParenthesis),
            new Lexer.Token("time", Lexer.TokenType.Identifier),
            new Lexer.Token(">=", Lexer.TokenType.ComparisonGreaterThanOrEqualTo),
            new Lexer.Token("25.0e0", Lexer.TokenType.Float),
            new Lexer.Token(")", Lexer.TokenType.RightParenthesis)
          },
          new object[]
          {
            "failed = (count + 1) * 5 / 2; failed > 10",
            new Lexer.Token("failed", Lexer.TokenType.Identifier),
            new Lexer.Token("=", Lexer.TokenType.AssignmentOperator),
            new Lexer.Token("(", Lexer.TokenType.LeftParenthesis),
            new Lexer.Token("count", Lexer.TokenType.Identifier),
            new Lexer.Token("+", Lexer.TokenType.PlusSign),
            new Lexer.Token("1", Lexer.TokenType.Integer),
            new Lexer.Token(")", Lexer.TokenType.RightParenthesis),
            new Lexer.Token("*", Lexer.TokenType.MultiplyOperator),
            new Lexer.Token("5", Lexer.TokenType.Integer),
            new Lexer.Token("/", Lexer.TokenType.DivideOperator),
            new Lexer.Token("2", Lexer.TokenType.Integer),
            new Lexer.Token(";", Lexer.TokenType.Semicolon),
            new Lexer.Token("failed", Lexer.TokenType.Identifier),
            new Lexer.Token(">", Lexer.TokenType.ComparisonGreaterThan),
            new Lexer.Token("10", Lexer.TokenType.Integer)
          },
          new object[]
          {
            "result = rand(0, 10)",
            new Lexer.Token("result", Lexer.TokenType.Identifier),
            new Lexer.Token("=", Lexer.TokenType.AssignmentOperator),
            new Lexer.Token("rand", Lexer.TokenType.Identifier),
            new Lexer.Token("(", Lexer.TokenType.LeftParenthesis),
            new Lexer.Token("0", Lexer.TokenType.Integer),
            new Lexer.Token(",", Lexer.TokenType.Comma),
            new Lexer.Token("10", Lexer.TokenType.Integer),
            new Lexer.Token(")", Lexer.TokenType.RightParenthesis)
          },
        };
      }
    }
  }
}
