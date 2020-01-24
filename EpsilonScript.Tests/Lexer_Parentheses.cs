using System.Collections.Generic;
using EpsilonScript.Lexer;
using Xunit;

namespace EpsilonScript.Tests
{
  public class Lexer_Parenthesiss : Lexer_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void Lexer_Parenthesis_Correctly(string input, params Token[] expected)
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
            new Token("(", TokenType.LeftParenthesis)
          },
          new object[]
          {
            ")",
            new Token(")", TokenType.RightParenthesis)
          },
          new object[]
          {
            " (",
            new Token("(", TokenType.LeftParenthesis)
          },
          new object[]
          {
            ") ",
            new Token(")", TokenType.RightParenthesis)
          },
          new object[]
          {
            "()",
            new Token("(", TokenType.LeftParenthesis),
            new Token(")", TokenType.RightParenthesis)
          },
          new object[]
          {
            ")(",
            new Token(")", TokenType.RightParenthesis),
            new Token("(", TokenType.LeftParenthesis)
          },
          new object[]
          {
            "()()()",
            new Token("(", TokenType.LeftParenthesis),
            new Token(")", TokenType.RightParenthesis),
            new Token("(", TokenType.LeftParenthesis),
            new Token(")", TokenType.RightParenthesis),
            new Token("(", TokenType.LeftParenthesis),
            new Token(")", TokenType.RightParenthesis)
          },
          new object[]
          {
            "(())())()",
            new Token("(", TokenType.LeftParenthesis),
            new Token("(", TokenType.LeftParenthesis),
            new Token(")", TokenType.RightParenthesis),
            new Token(")", TokenType.RightParenthesis),
            new Token("(", TokenType.LeftParenthesis),
            new Token(")", TokenType.RightParenthesis),
            new Token(")", TokenType.RightParenthesis),
            new Token("(", TokenType.LeftParenthesis),
            new Token(")", TokenType.RightParenthesis)
          },
        };
      }
    }
  }
}