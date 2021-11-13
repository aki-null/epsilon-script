using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;

namespace EpsilonScript.Tests
{
  public class RPNConverter_Function : RPNConverter_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void RPNConverter_ParsesFunction_Correctly(Element[] input, Element[] expected)
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
              new Element(new Token("func", TokenType.Identifier), ElementType.Function),
            },
            new Element[]
            {
              new Element(new Token("func", TokenType.Identifier), ElementType.Function),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("func", TokenType.Identifier), ElementType.Function),
              new Element(new Token("(", TokenType.LeftParenthesis), ElementType.FunctionStartParenthesis),
              new Element(new Token("", TokenType.None), ElementType.None),
              new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis),
            },
            new Element[]
            {
              new Element(new Token("", TokenType.None), ElementType.None),
              new Element(new Token("func", TokenType.Identifier), ElementType.Function),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("func", TokenType.Identifier), ElementType.Function),
              new Element(new Token("(", TokenType.LeftParenthesis), ElementType.FunctionStartParenthesis),
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("func", TokenType.Identifier), ElementType.Function),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("func", TokenType.Identifier), ElementType.Function),
              new Element(new Token("(", TokenType.LeftParenthesis), ElementType.FunctionStartParenthesis),
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token(",", TokenType.Comma), ElementType.Comma),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token(",", TokenType.Comma), ElementType.Comma),
              new Element(new Token("func", TokenType.Identifier), ElementType.Function),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("func", TokenType.Identifier), ElementType.Function),
              new Element(new Token("(", TokenType.LeftParenthesis), ElementType.FunctionStartParenthesis),
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token(",", TokenType.Comma), ElementType.Comma),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
              new Element(new Token("3", TokenType.Integer), ElementType.Integer),
              new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("3", TokenType.Integer), ElementType.Integer),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
              new Element(new Token(",", TokenType.Comma), ElementType.Comma),
              new Element(new Token("func", TokenType.Identifier), ElementType.Function),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
              new Element(new Token("func", TokenType.Identifier), ElementType.Function),
              new Element(new Token("(", TokenType.LeftParenthesis), ElementType.FunctionStartParenthesis),
              new Element(new Token("", TokenType.None), ElementType.None),
              new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("", TokenType.None), ElementType.None),
              new Element(new Token("func", TokenType.Identifier), ElementType.Function),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            }
          },
        };
      }
    }
  }
}