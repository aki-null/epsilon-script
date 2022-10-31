using System;
using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;

namespace EpsilonScript.Tests
{
  public class TokenParser_Identifier : TokenParser_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void RPNConverter_ParsesVariable_Correctly(Token[] input, Element[] expected)
    {
      Succeeds(input, expected);
    }

    public static IEnumerable<object[]> CorrectData
    {
      get
      {
        return new[]
        {
          CreateTestData(
            new Element(new Token("var", TokenType.Identifier), ElementType.Variable)
          ),
          CreateTestData(
            new Element(new Token("var", TokenType.Identifier), ElementType.Variable),
            new Element(new Token("=", TokenType.AssignmentOperator), ElementType.AssignmentOperator),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer)
          ),
          CreateTestData(
            new Element(new Token("var", TokenType.Identifier), ElementType.Variable),
            new Element(new Token("=", TokenType.AssignmentOperator), ElementType.AssignmentOperator),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer)
          ),
          CreateTestData(
            new Element(new Token("var", TokenType.Identifier), ElementType.Variable),
            new Element(new Token("*", TokenType.MultiplyOperator), ElementType.MultiplyOperator),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer)
          ),
          new object[]
          {
            new Token[]
            {
              new Token("var", TokenType.Identifier),
              new Token("(", TokenType.LeftParenthesis),
              new Token(")", TokenType.RightParenthesis),
            },
            new Element[]
            {
              new Element(new Token("var", TokenType.Identifier), ElementType.Function),
              new Element(new Token("(", TokenType.LeftParenthesis), ElementType.FunctionStartParenthesis),
              new Element(new Token(ReadOnlyMemory<char>.Empty, TokenType.None), ElementType.None),
              new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis),
            },
          },
          CreateTestData(
            new Element(new Token("(", TokenType.LeftParenthesis), ElementType.LeftParenthesis),
            new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis)
          ),
          CreateTestData(
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new Element(new Token("(", TokenType.LeftParenthesis), ElementType.LeftParenthesis),
            new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis)
          ),
          CreateTestData(
            new Element(new Token("var", TokenType.Identifier), ElementType.Function),
            new Element(new Token("(", TokenType.LeftParenthesis), ElementType.FunctionStartParenthesis),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis)
          ),
          CreateTestData(
            new Element(new Token("var", TokenType.Identifier), ElementType.Function),
            new Element(new Token("(", TokenType.LeftParenthesis), ElementType.FunctionStartParenthesis),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Element(new Token(",", TokenType.Comma), ElementType.Comma),
            new Element(new Token("2", TokenType.Integer), ElementType.Integer),
            new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis)
          ),
          CreateTestData(
            new Element(new Token("var", TokenType.Identifier), ElementType.Function),
            new Element(new Token("(", TokenType.LeftParenthesis), ElementType.FunctionStartParenthesis),
            new Element(new Token("\"Hello World\"", TokenType.String), ElementType.String),
            new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis)
          ),
          CreateTestData(
            new Element(new Token("var", TokenType.Identifier), ElementType.Function),
            new Element(new Token("(", TokenType.LeftParenthesis), ElementType.FunctionStartParenthesis),
            new Element(new Token("\"Hello World\"", TokenType.String), ElementType.String),
            new Element(new Token("\"こんにちは世界\"", TokenType.String), ElementType.String),
            new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis)
          ),
        };
      }
    }
  }
}