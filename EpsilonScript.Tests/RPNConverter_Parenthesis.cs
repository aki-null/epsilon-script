using System.Collections.Generic;
using EpsilonScript.Lexer;
using EpsilonScript.Parser;
using Xunit;

namespace EpsilonScript.Tests
{
  public class RPNConverter_Parenthesis : RPNConverter_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void RPNConverter_ParsesParenthesis_Correctly(Element[] input, Element[] expected)
    {
      Succeeds(input, expected);
    }

    [Theory]
    [MemberData(nameof(IncorrectData))]
    public void RPNConverter_ParsesParenthesis_Failes(Element[] input)
    {
      Fails(input);
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
              new Element(new Token("(", TokenType.LeftParenthesis), ElementType.LeftParenthesis),
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis),
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
              new Element(new Token("(", TokenType.LeftParenthesis), ElementType.LeftParenthesis),
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator)
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("(", TokenType.LeftParenthesis), ElementType.LeftParenthesis),
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis),
              new Element(new Token("*", TokenType.MultiplyOperator), ElementType.MultiplyOperator),
              new Element(new Token("3", TokenType.Integer), ElementType.Integer),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
              new Element(new Token("3", TokenType.Integer), ElementType.Integer),
              new Element(new Token("*", TokenType.MultiplyOperator), ElementType.MultiplyOperator),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("(", TokenType.LeftParenthesis), ElementType.LeftParenthesis),
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis),
              new Element(new Token("*", TokenType.MultiplyOperator), ElementType.MultiplyOperator),
              new Element(new Token("(", TokenType.LeftParenthesis), ElementType.LeftParenthesis),
              new Element(new Token("3", TokenType.Integer), ElementType.Integer),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
              new Element(new Token("4", TokenType.Integer), ElementType.Integer),
              new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
              new Element(new Token("3", TokenType.Integer), ElementType.Integer),
              new Element(new Token("4", TokenType.Integer), ElementType.Integer),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
              new Element(new Token("*", TokenType.MultiplyOperator), ElementType.MultiplyOperator),
            }
          },
        };
      }
    }

    public static IEnumerable<object[]> IncorrectData
    {
      get
      {
        return new[]
        {
          new object[]
          {
            new Element[]
            {
              new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("(", TokenType.LeftParenthesis), ElementType.LeftParenthesis),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("(", TokenType.LeftParenthesis), ElementType.LeftParenthesis),
              new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis),
              new Element(new Token("(", TokenType.LeftParenthesis), ElementType.LeftParenthesis),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("(", TokenType.LeftParenthesis), ElementType.LeftParenthesis),
              new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis),
              new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis),
            }
          },
        };
      }
    }
  }
}