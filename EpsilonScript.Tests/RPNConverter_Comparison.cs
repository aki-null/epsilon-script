using System.Collections.Generic;
using EpsilonScript.Lexer;
using EpsilonScript.Parser;
using Xunit;

namespace EpsilonScript.Tests
{
  public class RPNConverter_Comparison : RPNConverter_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void RPNConverter_ParsesComparison_Correctly(Element[] input, Element[] expected)
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
              new Element(new Token("<", TokenType.ComparisonLessThan), ElementType.ComparisonLessThan),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("<", TokenType.ComparisonLessThan), ElementType.ComparisonLessThan),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token(">", TokenType.ComparisonGreaterThan), ElementType.ComparisonGreaterThan),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token(">", TokenType.ComparisonGreaterThan), ElementType.ComparisonGreaterThan),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("<=", TokenType.ComparisonLessThanOrEqualTo),
                ElementType.ComparisonLessThanOrEqualTo),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("<=", TokenType.ComparisonLessThanOrEqualTo),
                ElementType.ComparisonLessThanOrEqualTo),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token(">=", TokenType.ComparisonGreaterThanOrEqualTo),
                ElementType.ComparisonGreaterThanOrEqualTo),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token(">=", TokenType.ComparisonGreaterThanOrEqualTo),
                ElementType.ComparisonGreaterThanOrEqualTo),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("==", TokenType.ComparisonGreaterThanOrEqualTo), ElementType.ComparisonEqual),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("==", TokenType.ComparisonGreaterThanOrEqualTo), ElementType.ComparisonEqual),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("!=", TokenType.ComparisonNotEqual), ElementType.ComparisonNotEqual),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("!=", TokenType.ComparisonNotEqual), ElementType.ComparisonNotEqual),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("result", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("==", TokenType.ComparisonEqual), ElementType.ComparisonEqual),
              new Element(new Token("true", TokenType.BooleanLiteralTrue), ElementType.BooleanLiteralTrue),
            },
            new Element[]
            {
              new Element(new Token("result", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("true", TokenType.BooleanLiteralTrue), ElementType.BooleanLiteralTrue),
              new Element(new Token("==", TokenType.ComparisonEqual), ElementType.ComparisonEqual),
            }
          },
        };
      }
    }
  }
}