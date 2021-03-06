using System.Collections.Generic;
using EpsilonScript.Lexer;
using EpsilonScript.Parser;
using Xunit;

namespace EpsilonScript.Tests
{
  public class RPNConverter_Math : RPNConverter_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void RPNConverter_ParsesMath_Correctly(Element[] input, Element[] expected)
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
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("1", TokenType.Float), ElementType.Float),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Float), ElementType.Float)
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
              new Element(new Token("3", TokenType.Integer), ElementType.Integer),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
              new Element(new Token("3", TokenType.Integer), ElementType.Integer),
              new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
              new Element(new Token("3", TokenType.Integer), ElementType.Integer),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
              new Element(new Token("3", TokenType.Integer), ElementType.Integer),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
              new Element(new Token("3", TokenType.Integer), ElementType.Integer),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
              new Element(new Token("3", TokenType.Integer), ElementType.Integer),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("*", TokenType.MultiplyOperator), ElementType.MultiplyOperator),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("*", TokenType.MultiplyOperator), ElementType.MultiplyOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("/", TokenType.DivideOperator), ElementType.DivideOperator),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("/", TokenType.DivideOperator), ElementType.DivideOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("/", TokenType.DivideOperator), ElementType.DivideOperator),
              new Element(new Token("3", TokenType.Integer), ElementType.Integer),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("3", TokenType.Integer), ElementType.Integer),
              new Element(new Token("/", TokenType.DivideOperator), ElementType.DivideOperator),
              new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("*", TokenType.DivideOperator), ElementType.DivideOperator),
              new Element(new Token("3", TokenType.Integer), ElementType.Integer),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("3", TokenType.Integer), ElementType.Integer),
              new Element(new Token("*", TokenType.DivideOperator), ElementType.DivideOperator),
              new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("*", TokenType.DivideOperator), ElementType.DivideOperator),
              new Element(new Token("3", TokenType.Integer), ElementType.Integer),
              new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            },
            new Element[]
            {
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("3", TokenType.Integer), ElementType.Integer),
              new Element(new Token("*", TokenType.DivideOperator), ElementType.DivideOperator),
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("/", TokenType.DivideOperator), ElementType.DivideOperator),
              new Element(new Token("3", TokenType.Integer), ElementType.Integer),
              new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            },
            new Element[]
            {
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("3", TokenType.Integer), ElementType.Integer),
              new Element(new Token("/", TokenType.DivideOperator), ElementType.DivideOperator),
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("/", TokenType.DivideOperator), ElementType.DivideOperator),
              new Element(new Token("3", TokenType.Integer), ElementType.Integer),
              new Element(new Token("*", TokenType.MultiplyOperator), ElementType.MultiplyOperator),
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            },
            new Element[]
            {
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("3", TokenType.Integer), ElementType.Integer),
              new Element(new Token("/", TokenType.DivideOperator), ElementType.DivideOperator),
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("*", TokenType.MultiplyOperator), ElementType.MultiplyOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("*", TokenType.MultiplyOperator), ElementType.MultiplyOperator),
              new Element(new Token("3", TokenType.Integer), ElementType.Integer),
              new Element(new Token("/", TokenType.DivideOperator), ElementType.DivideOperator),
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            },
            new Element[]
            {
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("3", TokenType.Integer), ElementType.Integer),
              new Element(new Token("*", TokenType.MultiplyOperator), ElementType.MultiplyOperator),
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("/", TokenType.DivideOperator), ElementType.DivideOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("*", TokenType.MultiplyOperator), ElementType.MultiplyOperator),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
              new Element(new Token("3", TokenType.Integer), ElementType.Integer),
              new Element(new Token("/", TokenType.DivideOperator), ElementType.DivideOperator),
              new Element(new Token("4", TokenType.Integer), ElementType.Integer),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("*", TokenType.MultiplyOperator), ElementType.MultiplyOperator),
              new Element(new Token("3", TokenType.Integer), ElementType.Integer),
              new Element(new Token("4", TokenType.Integer), ElementType.Integer),
              new Element(new Token("/", TokenType.DivideOperator), ElementType.DivideOperator),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("3", TokenType.Integer), ElementType.Integer),
              new Element(new Token("4", TokenType.Integer), ElementType.Integer),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("3", TokenType.Integer), ElementType.Integer),
              new Element(new Token("4", TokenType.Integer), ElementType.Integer),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
              new Element(new Token("3", TokenType.Integer), ElementType.Integer),
              new Element(new Token("4", TokenType.Integer), ElementType.Integer),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("3", TokenType.Integer), ElementType.Integer),
              new Element(new Token("4", TokenType.Integer), ElementType.Integer),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("%", TokenType.ModuloOperator), ElementType.ModuloOperator),
              new Element(new Token("5", TokenType.Integer), ElementType.Integer),
            },
            new Element[]
            {
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("5", TokenType.Integer), ElementType.Integer),
              new Element(new Token("%", TokenType.ModuloOperator), ElementType.ModuloOperator),
            }
          },
        };
      }
    }
  }
}