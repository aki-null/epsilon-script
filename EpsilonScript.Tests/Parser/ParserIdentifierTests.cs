using System;
using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.Parser
{
  public class ParserIdentifierTests : TokenParserTestBase
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    internal void Parser_ParsesIdentifier_Correctly(Token[] input, Element[] expected)
    {
      AssertParseSucceeds(input, expected);
    }

    public static IEnumerable<object[]> CorrectData
    {
      get
      {
        return new[]
        {
          CreateTestCase(
            new Element(new Token("var", TokenType.Identifier), ElementType.Variable)
          ),
          CreateTestCase(
            new Element(new Token("var", TokenType.Identifier), ElementType.Variable),
            new Element(new Token("=", TokenType.AssignmentOperator), ElementType.AssignmentOperator),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer)
          ),
          CreateTestCase(
            new Element(new Token("var", TokenType.Identifier), ElementType.Variable),
            new Element(new Token("=", TokenType.AssignmentOperator), ElementType.AssignmentOperator),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer)
          ),
          CreateTestCase(
            new Element(new Token("var", TokenType.Identifier), ElementType.Variable),
            new Element(new Token("*", TokenType.MultiplyOperator), ElementType.MultiplyOperator),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer)
          ),
          new object[]
          {
            new[]
            {
              new Token("var", TokenType.Identifier),
              new Token("(", TokenType.LeftParenthesis),
              new Token(")", TokenType.RightParenthesis),
            },
            new[]
            {
              new Element(new Token("var", TokenType.Identifier), ElementType.Function),
              new Element(new Token("(", TokenType.LeftParenthesis), ElementType.FunctionStartParenthesis),
              new Element(new Token(ReadOnlyMemory<char>.Empty, TokenType.None), ElementType.None),
              new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis),
            },
          },
          CreateTestCase(
            new Element(new Token("var", TokenType.Identifier), ElementType.Function),
            new Element(new Token("(", TokenType.LeftParenthesis), ElementType.FunctionStartParenthesis),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis)
          ),
          CreateTestCase(
            new Element(new Token("var", TokenType.Identifier), ElementType.Function),
            new Element(new Token("(", TokenType.LeftParenthesis), ElementType.FunctionStartParenthesis),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Element(new Token(",", TokenType.Comma), ElementType.Comma),
            new Element(new Token("2", TokenType.Integer), ElementType.Integer),
            new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis)
          ),
          CreateTestCase(
            new Element(new Token("var", TokenType.Identifier), ElementType.Function),
            new Element(new Token("(", TokenType.LeftParenthesis), ElementType.FunctionStartParenthesis),
            new Element(new Token("\"Hello World\"", TokenType.String), ElementType.String),
            new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis)
          ),
          CreateTestCase(
            new Element(new Token("var", TokenType.Identifier), ElementType.Function),
            new Element(new Token("(", TokenType.LeftParenthesis), ElementType.FunctionStartParenthesis),
            new Element(new Token("\"Hello World\"", TokenType.String), ElementType.String),
            new Element(new Token(",", TokenType.Comma), ElementType.Comma),
            new Element(new Token("\"こんにちは世界\"", TokenType.String), ElementType.String),
            new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis)
          ),
          // Multi-argument function calls
          CreateTestCase(
            new Element(new Token("sum", TokenType.Identifier), ElementType.Function),
            new Element(new Token("(", TokenType.LeftParenthesis), ElementType.FunctionStartParenthesis),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Element(new Token(",", TokenType.Comma), ElementType.Comma),
            new Element(new Token("2", TokenType.Integer), ElementType.Integer),
            new Element(new Token(",", TokenType.Comma), ElementType.Comma),
            new Element(new Token("3", TokenType.Integer), ElementType.Integer),
            new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis)
          ),
          CreateTestCase(
            new Element(new Token("calc", TokenType.Identifier), ElementType.Function),
            new Element(new Token("(", TokenType.LeftParenthesis), ElementType.FunctionStartParenthesis),
            new Element(new Token("x", TokenType.Identifier), ElementType.Variable),
            new Element(new Token(",", TokenType.Comma), ElementType.Comma),
            new Element(new Token("true", TokenType.BooleanLiteralTrue), ElementType.BooleanLiteralTrue),
            new Element(new Token(",", TokenType.Comma), ElementType.Comma),
            new Element(new Token("\"text\"", TokenType.String), ElementType.String),
            new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis)
          ),
        };
      }
    }
  }
}