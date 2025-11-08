using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.RpnConverter
{
  public class RpnFunctionTests : RpnConverterTestBase
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    internal void RpnConverter_FunctionCalls_ConvertCorrectly(Element[] input, Element[] expected)
    {
      AssertRpnSucceeds(input, expected);
    }

    public static IEnumerable<object[]> CorrectData
    {
      get
      {
        return new[]
        {
          new object[]
          {
            new[]
            {
              new Element(new Token("func", TokenType.Identifier), ElementType.Function),
            },
            new[]
            {
              new Element(new Token("func", TokenType.Identifier), ElementType.Function),
            }
          },
          new object[]
          {
            new[]
            {
              new Element(new Token("func", TokenType.Identifier), ElementType.Function),
              new Element(new Token("(", TokenType.LeftParenthesis), ElementType.FunctionStartParenthesis),
              new Element(new Token("", TokenType.None), ElementType.None),
              new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis),
            },
            new[]
            {
              new Element(new Token("", TokenType.None), ElementType.None),
              new Element(new Token("func", TokenType.Identifier), ElementType.Function),
            }
          },
          new object[]
          {
            new[]
            {
              new Element(new Token("func", TokenType.Identifier), ElementType.Function),
              new Element(new Token("(", TokenType.LeftParenthesis), ElementType.FunctionStartParenthesis),
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis),
            },
            new[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("func", TokenType.Identifier), ElementType.Function),
            }
          },
          new object[]
          {
            new[]
            {
              new Element(new Token("func", TokenType.Identifier), ElementType.Function),
              new Element(new Token("(", TokenType.LeftParenthesis), ElementType.FunctionStartParenthesis),
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token(",", TokenType.Comma), ElementType.Comma),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis),
            },
            new[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token(",", TokenType.Comma), ElementType.Comma),
              new Element(new Token("func", TokenType.Identifier), ElementType.Function),
            }
          },
          new object[]
          {
            new[]
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
            new[]
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
            new[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
              new Element(new Token("func", TokenType.Identifier), ElementType.Function),
              new Element(new Token("(", TokenType.LeftParenthesis), ElementType.FunctionStartParenthesis),
              new Element(new Token("", TokenType.None), ElementType.None),
              new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis),
            },
            new[]
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