using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;

namespace EpsilonScript.Tests
{
  public class RPNConverter_Assignment : RPNConverter_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    internal void RPNConverter_ParsesAssignment_Correctly(Element[] input, Element[] expected)
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
            new Element[]
            {
              new Element(new Token("=", TokenType.AssignmentOperator), ElementType.AssignmentOperator),
            },
            new Element[]
            {
              new Element(new Token("=", TokenType.AssignmentOperator), ElementType.AssignmentOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("left", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("=", TokenType.AssignmentOperator), ElementType.AssignmentOperator),
              new Element(new Token("right", TokenType.Identifier), ElementType.Variable),
            },
            new Element[]
            {
              new Element(new Token("left", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("right", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("=", TokenType.AssignmentOperator), ElementType.AssignmentOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("left", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("=", TokenType.AssignmentOperator), ElementType.AssignmentOperator),
              new Element(new Token("right1", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
              new Element(new Token("right2", TokenType.Identifier), ElementType.Variable),
            },
            new Element[]
            {
              new Element(new Token("left", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("right1", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("right2", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
              new Element(new Token("=", TokenType.AssignmentOperator), ElementType.AssignmentOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("+=", TokenType.AssignmentAddOperator), ElementType.AssignmentAddOperator),
            },
            new Element[]
            {
              new Element(new Token("+=", TokenType.AssignmentAddOperator), ElementType.AssignmentAddOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("left", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("+=", TokenType.AssignmentAddOperator), ElementType.AssignmentAddOperator),
              new Element(new Token("right", TokenType.Identifier), ElementType.Variable),
            },
            new Element[]
            {
              new Element(new Token("left", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("right", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("+=", TokenType.AssignmentAddOperator), ElementType.AssignmentAddOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("left", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("+=", TokenType.AssignmentAddOperator), ElementType.AssignmentAddOperator),
              new Element(new Token("right1", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
              new Element(new Token("right2", TokenType.Identifier), ElementType.Variable),
            },
            new Element[]
            {
              new Element(new Token("left", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("right1", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("right2", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
              new Element(new Token("+=", TokenType.AssignmentAddOperator), ElementType.AssignmentAddOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("-=", TokenType.AssignmentSubtractOperator),
                ElementType.AssignmentSubtractOperator),
            },
            new Element[]
            {
              new Element(new Token("-=", TokenType.AssignmentSubtractOperator),
                ElementType.AssignmentSubtractOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("left", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("-=", TokenType.AssignmentSubtractOperator),
                ElementType.AssignmentSubtractOperator),
              new Element(new Token("right", TokenType.Identifier), ElementType.Variable),
            },
            new Element[]
            {
              new Element(new Token("left", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("right", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("-=", TokenType.AssignmentSubtractOperator),
                ElementType.AssignmentSubtractOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("left", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("-=", TokenType.AssignmentSubtractOperator),
                ElementType.AssignmentSubtractOperator),
              new Element(new Token("right1", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
              new Element(new Token("right2", TokenType.Identifier), ElementType.Variable),
            },
            new Element[]
            {
              new Element(new Token("left", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("right1", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("right2", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
              new Element(new Token("-=", TokenType.AssignmentSubtractOperator),
                ElementType.AssignmentSubtractOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("*=", TokenType.AssignmentMultiplyOperator),
                ElementType.AssignmentMultiplyOperator),
            },
            new Element[]
            {
              new Element(new Token("*=", TokenType.AssignmentMultiplyOperator),
                ElementType.AssignmentMultiplyOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("left", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("*=", TokenType.AssignmentMultiplyOperator),
                ElementType.AssignmentMultiplyOperator),
              new Element(new Token("right", TokenType.Identifier), ElementType.Variable),
            },
            new Element[]
            {
              new Element(new Token("left", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("right", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("*=", TokenType.AssignmentMultiplyOperator),
                ElementType.AssignmentMultiplyOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("left", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("*=", TokenType.AssignmentMultiplyOperator),
                ElementType.AssignmentMultiplyOperator),
              new Element(new Token("right1", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
              new Element(new Token("right2", TokenType.Identifier), ElementType.Variable),
            },
            new Element[]
            {
              new Element(new Token("left", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("right1", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("right2", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
              new Element(new Token("*=", TokenType.AssignmentMultiplyOperator),
                ElementType.AssignmentMultiplyOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("/=", TokenType.AssignmentDivideOperator), ElementType.AssignmentDivideOperator),
            },
            new Element[]
            {
              new Element(new Token("/=", TokenType.AssignmentDivideOperator), ElementType.AssignmentDivideOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("left", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("/=", TokenType.AssignmentDivideOperator), ElementType.AssignmentDivideOperator),
              new Element(new Token("right", TokenType.Identifier), ElementType.Variable),
            },
            new Element[]
            {
              new Element(new Token("left", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("right", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("/=", TokenType.AssignmentDivideOperator), ElementType.AssignmentDivideOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("left", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("/=", TokenType.AssignmentDivideOperator), ElementType.AssignmentDivideOperator),
              new Element(new Token("right1", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
              new Element(new Token("right2", TokenType.Identifier), ElementType.Variable),
            },
            new Element[]
            {
              new Element(new Token("left", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("right1", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("right2", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
              new Element(new Token("/=", TokenType.AssignmentDivideOperator), ElementType.AssignmentDivideOperator),
            }
          },
        };
      }
    }
  }
}