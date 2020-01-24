using System.Collections.Generic;
using EpsilonScript.Lexer;
using Xunit;

namespace EpsilonScript.Tests
{
  public class Lexer_BooleanOperator : Lexer_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void Lexer_BooleanOperator_Correctly(string input, params Token[] expected)
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
            new Token("||", TokenType.BooleanOrOperator)
          },
          new object[]
          {
            "|| ||",
            new Token("||", TokenType.BooleanOrOperator),
            new Token("||", TokenType.BooleanOrOperator)
          },
          new object[]
          {
            "||||",
            new Token("||", TokenType.BooleanOrOperator),
            new Token("||", TokenType.BooleanOrOperator)
          },
          new object[]
          {
            "&&",
            new Token("&&", TokenType.BooleanAndOperator)
          },
          new object[]
          {
            "&& &&",
            new Token("&&", TokenType.BooleanAndOperator),
            new Token("&&", TokenType.BooleanAndOperator)
          },
          new object[]
          {
            "&&&&",
            new Token("&&", TokenType.BooleanAndOperator),
            new Token("&&", TokenType.BooleanAndOperator)
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
          new object[] {"&"},
          new object[] {" &"},
          new object[] {"& "},
          new object[] {"& &"},
          new object[] {"|"},
          new object[] {" |"},
          new object[] {"| "},
          new object[] {"| |"},
        };
      }
    }
  }
}