using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;
using EpsilonScript.Tests.TestInfrastructure.Fakes;

namespace EpsilonScript.Tests.Lexer
{
  public class Lexer_BooleanOperator : LexerTestBase
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void Lexer_BooleanOperator_Correctly(string input, params Token[] expected)
    {
      AssertLexSucceeds(input, expected);
    }

    [Theory]
    [MemberData(nameof(IncorrectData))]
    public void Lexer_BooleanOperator_AssertLexFails(string input)
    {
      AssertLexFails(input);
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