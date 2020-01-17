using System.Collections.Generic;
using Xunit;

namespace EpsilonScript.Tests
{
  public class Lexer_BooleanOperator : Lexer_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void Lexer_BooleanOperator_Correctly(string input, params Lexer.Token[] expected)
    {
      Succeeds(input, expected);
    }

    [Theory]
    [MemberData(nameof(IncorrectData))]
    public void Lexer_BooleanOperator_Fails(string input)
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
            "||",
            new Lexer.Token("||", Lexer.TokenType.BooleanOrOperator)
          },
          new object[]
          {
            "|| ||",
            new Lexer.Token("||", Lexer.TokenType.BooleanOrOperator),
            new Lexer.Token("||", Lexer.TokenType.BooleanOrOperator)
          },
          new object[]
          {
            "||||",
            new Lexer.Token("||", Lexer.TokenType.BooleanOrOperator),
            new Lexer.Token("||", Lexer.TokenType.BooleanOrOperator)
          },
          new object[]
          {
            "&&",
            new Lexer.Token("&&", Lexer.TokenType.BooleanAndOperator)
          },
          new object[]
          {
            "&& &&",
            new Lexer.Token("&&", Lexer.TokenType.BooleanAndOperator),
            new Lexer.Token("&&", Lexer.TokenType.BooleanAndOperator)
          },
          new object[]
          {
            "&&&&",
            new Lexer.Token("&&", Lexer.TokenType.BooleanAndOperator),
            new Lexer.Token("&&", Lexer.TokenType.BooleanAndOperator)
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
          new object[] { "&" },
          new object[] { " &" },
          new object[] { "& " },
          new object[] { "& &" },
          new object[] { "|" },
          new object[] { " |" },
          new object[] { "| " },
          new object[] { "| |" },
        };
      }
    }
  }
}
