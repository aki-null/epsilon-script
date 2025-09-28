using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.Lexer
{
  [Trait("Category", "Unit")]
  [Trait("Component", "Lexer")]
  public class Lexer_Assignment : LexerTestBase
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void Lexer_Assignment_Correctly(string input, params Token[] expected)
    {
      AssertLexSucceeds(input, expected);
    }

    public static IEnumerable<object[]> CorrectData
    {
      get
      {
        return new[]
        {
          // Basic assignment operators
          new object[]
          {
            "=",
            new Token("=", TokenType.AssignmentOperator)
          },
          new object[]
          {
            "+=",
            new Token("+=", TokenType.AssignmentAddOperator)
          },
          new object[]
          {
            "-=",
            new Token("-=", TokenType.AssignmentSubtractOperator)
          },
          new object[]
          {
            "*=",
            new Token("*=", TokenType.AssignmentMultiplyOperator)
          },
          new object[]
          {
            "/=",
            new Token("/=", TokenType.AssignmentDivideOperator)
          },
          // Multiple assignment operators
          new object[]
          {
            "= +=",
            new Token("=", TokenType.AssignmentOperator),
            new Token("+=", TokenType.AssignmentAddOperator)
          },
          new object[]
          {
            "*=/=",
            new Token("*=", TokenType.AssignmentMultiplyOperator),
            new Token("/=", TokenType.AssignmentDivideOperator)
          },
        };
      }
    }
  }
}