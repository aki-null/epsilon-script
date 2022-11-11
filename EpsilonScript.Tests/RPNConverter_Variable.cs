using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;

namespace EpsilonScript.Tests
{
  public class RPNConverter_Variable : RPNConverter_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    internal void RPNConverter_ParsesVariable_Correctly(Element[] input, Element[] expected)
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
              new Element(new Token("var", TokenType.Identifier), ElementType.Variable),
            },
            new Element[]
            {
              new Element(new Token("var", TokenType.Identifier), ElementType.Variable),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("var", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("var", TokenType.Identifier), ElementType.Variable),
            },
            new Element[]
            {
              new Element(new Token("var", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("var", TokenType.Identifier), ElementType.Variable),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("var", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
              new Element(new Token("var", TokenType.Identifier), ElementType.Variable),
            },
            new Element[]
            {
              new Element(new Token("var", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("var", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            }
          },
        };
      }
    }
  }
}