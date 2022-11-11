using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;

namespace EpsilonScript.Tests
{
  public class RPNConverter_Sign : RPNConverter_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    internal void RPNConverter_ParsesSign_Correctly(Element[] input, Element[] expected)
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
              new Element(new Token("-", TokenType.MinusSign), ElementType.NegativeOperator),
            },
            new Element[]
            {
              new Element(new Token("-", TokenType.MinusSign), ElementType.NegativeOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("+", TokenType.PlusSign), ElementType.PositiveOperator),
            },
            new Element[]
            {
              new Element(new Token("+", TokenType.PlusSign), ElementType.PositiveOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("-", TokenType.MinusSign), ElementType.NegativeOperator),
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("-", TokenType.MinusSign), ElementType.NegativeOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("+", TokenType.PlusSign), ElementType.PositiveOperator),
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("+", TokenType.PlusSign), ElementType.PositiveOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("-", TokenType.MinusSign), ElementType.NegativeOperator),
              new Element(new Token("-", TokenType.MinusSign), ElementType.NegativeOperator),
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("-", TokenType.MinusSign), ElementType.NegativeOperator),
              new Element(new Token("-", TokenType.MinusSign), ElementType.NegativeOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("+", TokenType.PlusSign), ElementType.PositiveOperator),
              new Element(new Token("+", TokenType.PlusSign), ElementType.PositiveOperator),
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("+", TokenType.PlusSign), ElementType.PositiveOperator),
              new Element(new Token("+", TokenType.PlusSign), ElementType.PositiveOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("-", TokenType.MinusSign), ElementType.NegativeOperator),
              new Element(new Token("+", TokenType.PlusSign), ElementType.PositiveOperator),
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("+", TokenType.PlusSign), ElementType.PositiveOperator),
              new Element(new Token("-", TokenType.MinusSign), ElementType.NegativeOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("+", TokenType.PlusSign), ElementType.PositiveOperator),
              new Element(new Token("-", TokenType.MinusSign), ElementType.NegativeOperator),
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("-", TokenType.MinusSign), ElementType.NegativeOperator),
              new Element(new Token("+", TokenType.PlusSign), ElementType.PositiveOperator),
            }
          },
        };
      }
    }
  }
}