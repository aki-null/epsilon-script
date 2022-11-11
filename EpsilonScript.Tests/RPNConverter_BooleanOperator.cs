using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;

namespace EpsilonScript.Tests
{
  public class RPNConverter_BooleanOperator : RPNConverter_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    internal void RPNConverter_ParsesBooleanOperator_Correctly(Element[] input, Element[] expected)
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
              new Element(new Token("true", TokenType.BooleanLiteralTrue), ElementType.BooleanLiteralTrue),
              new Element(new Token("||", TokenType.BooleanOrOperator), ElementType.BooleanOrOperator),
              new Element(new Token("false", TokenType.BooleanLiteralFalse), ElementType.BooleanLiteralFalse),
            },
            new Element[]
            {
              new Element(new Token("true", TokenType.BooleanLiteralTrue), ElementType.BooleanLiteralTrue),
              new Element(new Token("false", TokenType.BooleanLiteralFalse), ElementType.BooleanLiteralFalse),
              new Element(new Token("||", TokenType.BooleanOrOperator), ElementType.BooleanOrOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("true", TokenType.BooleanLiteralTrue), ElementType.BooleanLiteralTrue),
              new Element(new Token("&&", TokenType.BooleanAndOperator), ElementType.BooleanAndOperator),
              new Element(new Token("false", TokenType.BooleanLiteralFalse), ElementType.BooleanLiteralFalse),
            },
            new Element[]
            {
              new Element(new Token("true", TokenType.BooleanLiteralTrue), ElementType.BooleanLiteralTrue),
              new Element(new Token("false", TokenType.BooleanLiteralFalse), ElementType.BooleanLiteralFalse),
              new Element(new Token("&&", TokenType.BooleanAndOperator), ElementType.BooleanAndOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("true", TokenType.BooleanLiteralTrue), ElementType.BooleanLiteralTrue),
              new Element(new Token("&&", TokenType.BooleanAndOperator), ElementType.BooleanAndOperator),
              new Element(new Token("false", TokenType.BooleanLiteralFalse), ElementType.BooleanLiteralFalse),
              new Element(new Token("||", TokenType.BooleanOrOperator), ElementType.BooleanOrOperator),
              new Element(new Token("true", TokenType.BooleanLiteralTrue), ElementType.BooleanLiteralTrue),
            },
            new Element[]
            {
              new Element(new Token("true", TokenType.BooleanLiteralTrue), ElementType.BooleanLiteralTrue),
              new Element(new Token("false", TokenType.BooleanLiteralFalse), ElementType.BooleanLiteralFalse),
              new Element(new Token("&&", TokenType.BooleanAndOperator), ElementType.BooleanAndOperator),
              new Element(new Token("true", TokenType.BooleanLiteralTrue), ElementType.BooleanLiteralTrue),
              new Element(new Token("||", TokenType.BooleanOrOperator), ElementType.BooleanOrOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("true", TokenType.BooleanLiteralTrue), ElementType.BooleanLiteralTrue),
              new Element(new Token("||", TokenType.BooleanOrOperator), ElementType.BooleanOrOperator),
              new Element(new Token("true", TokenType.BooleanLiteralTrue), ElementType.BooleanLiteralTrue),
              new Element(new Token("&&", TokenType.BooleanAndOperator), ElementType.BooleanAndOperator),
              new Element(new Token("false", TokenType.BooleanLiteralFalse), ElementType.BooleanLiteralFalse),
            },
            new Element[]
            {
              new Element(new Token("true", TokenType.BooleanLiteralTrue), ElementType.BooleanLiteralTrue),
              new Element(new Token("true", TokenType.BooleanLiteralTrue), ElementType.BooleanLiteralTrue),
              new Element(new Token("false", TokenType.BooleanLiteralFalse), ElementType.BooleanLiteralFalse),
              new Element(new Token("&&", TokenType.BooleanAndOperator), ElementType.BooleanAndOperator),
              new Element(new Token("||", TokenType.BooleanOrOperator), ElementType.BooleanOrOperator),
            }
          },
        };
      }
    }
  }
}