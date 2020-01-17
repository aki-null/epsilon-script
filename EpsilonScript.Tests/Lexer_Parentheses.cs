using System.Collections.Generic;
using Xunit;

namespace EpsilonScript.Tests
{
  public class Lexer_Parenthesiss : Lexer_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void Lexer_Parenthesis_Correctly(string input, params Lexer.Token[] expected)
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
            "(",
            new Lexer.Token("(", Lexer.TokenType.LeftParenthesis)
          },
          new object[]
          {
            ")",
            new Lexer.Token(")", Lexer.TokenType.RightParenthesis)
          },
          new object[]
          {
            " (",
            new Lexer.Token("(", Lexer.TokenType.LeftParenthesis)
          },
          new object[]
          {
            ") ",
            new Lexer.Token(")", Lexer.TokenType.RightParenthesis)
          },
          new object[]
          {
            "()",
            new Lexer.Token("(", Lexer.TokenType.LeftParenthesis),
            new Lexer.Token(")", Lexer.TokenType.RightParenthesis)
          },
          new object[]
          {
            ")(",
            new Lexer.Token(")", Lexer.TokenType.RightParenthesis),
            new Lexer.Token("(", Lexer.TokenType.LeftParenthesis)
          },
          new object[]
          {
            "()()()",
            new Lexer.Token("(", Lexer.TokenType.LeftParenthesis),
            new Lexer.Token(")", Lexer.TokenType.RightParenthesis),
            new Lexer.Token("(", Lexer.TokenType.LeftParenthesis),
            new Lexer.Token(")", Lexer.TokenType.RightParenthesis),
            new Lexer.Token("(", Lexer.TokenType.LeftParenthesis),
            new Lexer.Token(")", Lexer.TokenType.RightParenthesis)
          },
          new object[]
          {
            "(())())()",
            new Lexer.Token("(", Lexer.TokenType.LeftParenthesis),
            new Lexer.Token("(", Lexer.TokenType.LeftParenthesis),
            new Lexer.Token(")", Lexer.TokenType.RightParenthesis),
            new Lexer.Token(")", Lexer.TokenType.RightParenthesis),
            new Lexer.Token("(", Lexer.TokenType.LeftParenthesis),
            new Lexer.Token(")", Lexer.TokenType.RightParenthesis),
            new Lexer.Token(")", Lexer.TokenType.RightParenthesis),
            new Lexer.Token("(", Lexer.TokenType.LeftParenthesis),
            new Lexer.Token(")", Lexer.TokenType.RightParenthesis)
          },
        };
      }
    }
  }
}
