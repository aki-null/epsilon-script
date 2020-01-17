using System.Collections.Generic;
using Xunit;

namespace EpsilonScript.Tests
{
  public class Lexer_Assignment : Lexer_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void Lexer_Assignment_Correctly(string input, params Lexer.Token[] expected)
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
            "=",
            new Lexer.Token("=", Lexer.TokenType.AssignmentOperator)
          },
          new object[]
          {
            " =",
            new Lexer.Token("=", Lexer.TokenType.AssignmentOperator)
          },
          new object[]
          {
            "= ",
            new Lexer.Token("=", Lexer.TokenType.AssignmentOperator)
          },
          new object[]
          {
            "= =",
            new Lexer.Token("=", Lexer.TokenType.AssignmentOperator),
            new Lexer.Token("=", Lexer.TokenType.AssignmentOperator),
          },
          new object[]
          {
            "+=",
            new Lexer.Token("+=", Lexer.TokenType.AssignmentAddOperator)
          },
          new object[]
          {
            " +=",
            new Lexer.Token("+=", Lexer.TokenType.AssignmentAddOperator)
          },
          new object[]
          {
            "+= ",
            new Lexer.Token("+=", Lexer.TokenType.AssignmentAddOperator)
          },
          new object[]
          {
            "+= +=",
            new Lexer.Token("+=", Lexer.TokenType.AssignmentAddOperator),
            new Lexer.Token("+=", Lexer.TokenType.AssignmentAddOperator)
          },
          new object[]
          {
            "+=+=",
            new Lexer.Token("+=", Lexer.TokenType.AssignmentAddOperator),
            new Lexer.Token("+=", Lexer.TokenType.AssignmentAddOperator)
          },
          new object[]
          {
            "-=",
            new Lexer.Token("-=", Lexer.TokenType.AssignmentSubtractOperator)
          },
          new object[]
          {
            " -=",
            new Lexer.Token("-=", Lexer.TokenType.AssignmentSubtractOperator)
          },
          new object[]
          {
            "-= ",
            new Lexer.Token("-=", Lexer.TokenType.AssignmentSubtractOperator)
          },
          new object[]
          {
            "-= -=",
            new Lexer.Token("-=", Lexer.TokenType.AssignmentSubtractOperator),
            new Lexer.Token("-=", Lexer.TokenType.AssignmentSubtractOperator)
          },
          new object[]
          {
            "-=-=",
            new Lexer.Token("-=", Lexer.TokenType.AssignmentSubtractOperator),
            new Lexer.Token("-=", Lexer.TokenType.AssignmentSubtractOperator)
          },
          new object[]
          {
            "*=",
            new Lexer.Token("*=", Lexer.TokenType.AssignmentMultiplyOperator)
          },
          new object[]
          {
            " *=",
            new Lexer.Token("*=", Lexer.TokenType.AssignmentMultiplyOperator)
          },
          new object[]
          {
            "*= ",
            new Lexer.Token("*=", Lexer.TokenType.AssignmentMultiplyOperator)
          },
          new object[]
          {
            "*= *=",
            new Lexer.Token("*=", Lexer.TokenType.AssignmentMultiplyOperator),
            new Lexer.Token("*=", Lexer.TokenType.AssignmentMultiplyOperator)
          },
          new object[]
          {
            "*=*=",
            new Lexer.Token("*=", Lexer.TokenType.AssignmentMultiplyOperator),
            new Lexer.Token("*=", Lexer.TokenType.AssignmentMultiplyOperator)
          },
          new object[]
          {
            "/=",
            new Lexer.Token("/=", Lexer.TokenType.AssignmentDivideOperator)
          },
          new object[]
          {
            " /=",
            new Lexer.Token("/=", Lexer.TokenType.AssignmentDivideOperator)
          },
          new object[]
          {
            "/= ",
            new Lexer.Token("/=", Lexer.TokenType.AssignmentDivideOperator)
          },
          new object[]
          {
            "/= /=",
            new Lexer.Token("/=", Lexer.TokenType.AssignmentDivideOperator),
            new Lexer.Token("/=", Lexer.TokenType.AssignmentDivideOperator)
          },
          new object[]
          {
            "/=/=",
            new Lexer.Token("/=", Lexer.TokenType.AssignmentDivideOperator),
            new Lexer.Token("/=", Lexer.TokenType.AssignmentDivideOperator)
          },
        };
      }
    }
  }
}
