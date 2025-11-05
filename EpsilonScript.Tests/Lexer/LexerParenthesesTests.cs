using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.Lexer
{
  [Trait("Category", "Unit")]
  [Trait("Component", "Lexer")]
  public class LexerParenthesesTests : LexerTestBase
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    internal void Lexer_Parenthesis_Correctly(string input, params Token[] expected)
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