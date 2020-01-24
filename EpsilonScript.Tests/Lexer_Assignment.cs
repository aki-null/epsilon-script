using System.Collections.Generic;
using EpsilonScript.Lexer;
using Xunit;

namespace EpsilonScript.Tests
{
  public class Lexer_Assignment : Lexer_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void Lexer_Assignment_Correctly(string input, params Token[] expected)
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
            new Token("=", TokenType.AssignmentOperator)
          },
          new object[]
          {
            " =",
            new Token("=", TokenType.AssignmentOperator)
          },
          new object[]
          {
            "= ",
            new Token("=", TokenType.AssignmentOperator)
          },
          new object[]
          {
            "= =",
            new Token("=", TokenType.AssignmentOperator),
            new Token("=", TokenType.AssignmentOperator),
          },
          new object[]
          {
            "+=",
            new Token("+=", TokenType.AssignmentAddOperator)
          },
          new object[]
          {
            " +=",
            new Token("+=", TokenType.AssignmentAddOperator)
          },
          new object[]
          {
            "+= ",
            new Token("+=", TokenType.AssignmentAddOperator)
          },
          new object[]
          {
            "+= +=",
            new Token("+=", TokenType.AssignmentAddOperator),
            new Token("+=", TokenType.AssignmentAddOperator)
          },
          new object[]
          {
            "+=+=",
            new Token("+=", TokenType.AssignmentAddOperator),
            new Token("+=", TokenType.AssignmentAddOperator)
          },
          new object[]
          {
            "-=",
            new Token("-=", TokenType.AssignmentSubtractOperator)
          },
          new object[]
          {
            " -=",
            new Token("-=", TokenType.AssignmentSubtractOperator)
          },
          new object[]
          {
            "-= ",
            new Token("-=", TokenType.AssignmentSubtractOperator)
          },
          new object[]
          {
            "-= -=",
            new Token("-=", TokenType.AssignmentSubtractOperator),
            new Token("-=", TokenType.AssignmentSubtractOperator)
          },
          new object[]
          {
            "-=-=",
            new Token("-=", TokenType.AssignmentSubtractOperator),
            new Token("-=", TokenType.AssignmentSubtractOperator)
          },
          new object[]
          {
            "*=",
            new Token("*=", TokenType.AssignmentMultiplyOperator)
          },
          new object[]
          {
            " *=",
            new Token("*=", TokenType.AssignmentMultiplyOperator)
          },
          new object[]
          {
            "*= ",
            new Token("*=", TokenType.AssignmentMultiplyOperator)
          },
          new object[]
          {
            "*= *=",
            new Token("*=", TokenType.AssignmentMultiplyOperator),
            new Token("*=", TokenType.AssignmentMultiplyOperator)
          },
          new object[]
          {
            "*=*=",
            new Token("*=", TokenType.AssignmentMultiplyOperator),
            new Token("*=", TokenType.AssignmentMultiplyOperator)
          },
          new object[]
          {
            "/=",
            new Token("/=", TokenType.AssignmentDivideOperator)
          },
          new object[]
          {
            " /=",
            new Token("/=", TokenType.AssignmentDivideOperator)
          },
          new object[]
          {
            "/= ",
            new Token("/=", TokenType.AssignmentDivideOperator)
          },
          new object[]
          {
            "/= /=",
            new Token("/=", TokenType.AssignmentDivideOperator),
            new Token("/=", TokenType.AssignmentDivideOperator)
          },
          new object[]
          {
            "/=/=",
            new Token("/=", TokenType.AssignmentDivideOperator),
            new Token("/=", TokenType.AssignmentDivideOperator)
          },
        };
      }
    }
  }
}