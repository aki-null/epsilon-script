using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;

namespace EpsilonScript.Tests
{
  public class RPNConverter_Negate : RPNConverter_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    internal void RPNConverter_ParsesNegate_Correctly(Element[] input, Element[] expected)
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
              new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator),
              new Element(new Token("var", TokenType.Identifier), ElementType.Variable),
            },
            new Element[]
            {
              new Element(new Token("var", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator),
              new Element(new Token("true", TokenType.BooleanLiteralTrue), ElementType.BooleanLiteralTrue),
            },
            new Element[]
            {
              new Element(new Token("true", TokenType.BooleanLiteralTrue), ElementType.BooleanLiteralTrue),
              new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator),
              new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator),
              new Element(new Token("true", TokenType.BooleanLiteralTrue), ElementType.BooleanLiteralTrue),
            },
            new Element[]
            {
              new Element(new Token("true", TokenType.BooleanLiteralTrue), ElementType.BooleanLiteralTrue),
              new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator),
              new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator),
              new Element(new Token("(", TokenType.LeftParenthesis), ElementType.LeftParenthesis),
              new Element(new Token("true", TokenType.BooleanLiteralTrue), ElementType.BooleanLiteralTrue),
              new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis),
            },
            new Element[]
            {
              new Element(new Token("true", TokenType.BooleanLiteralTrue), ElementType.BooleanLiteralTrue),
              new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator),
            }
          },
        };
      }
    }
  }
}