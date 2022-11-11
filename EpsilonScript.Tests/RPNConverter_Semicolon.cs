using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;

namespace EpsilonScript.Tests
{
  public class RPNConverter_Semicolon : RPNConverter_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    internal void RPNConverter_ParsesSemicolon_Correctly(Element[] input, Element[] expected)
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
              new Element(new Token(";", TokenType.Semicolon), ElementType.Semicolon),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token(";", TokenType.Semicolon), ElementType.Semicolon),
            }
          },
          new object[]
          {
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token(";", TokenType.Semicolon), ElementType.Semicolon),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token(";", TokenType.Semicolon), ElementType.Semicolon),
              new Element(new Token("3", TokenType.Integer), ElementType.Integer),
            },
            new Element[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token(";", TokenType.Semicolon), ElementType.Semicolon),
              new Element(new Token("3", TokenType.Integer), ElementType.Integer),
              new Element(new Token(";", TokenType.Semicolon), ElementType.Semicolon),
            }
          },
        };
      }
    }
  }
}