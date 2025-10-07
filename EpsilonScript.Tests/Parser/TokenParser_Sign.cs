using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.Parser
{
  [Trait("Category", "Unit")]
  [Trait("Component", "Parser")]
  public class TokenParser_Sign : TokenParserTestBase
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    internal void RPNConverter_ParsesSign_Correctly(Token[] input, Element[] expected)
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
            new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator)
          ),
          CreateTestCase(
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator)
          ),
          CreateTestCase(
            new Element(new Token("-", TokenType.MinusSign), ElementType.NegativeOperator),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer)
          ),
          CreateTestCase(
            new Element(new Token("+", TokenType.PlusSign), ElementType.PositiveOperator),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer)
          ),
          CreateTestCase(
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
            new Element(new Token("2", TokenType.Integer), ElementType.Integer)
          ),
          CreateTestCase(
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new Element(new Token("2", TokenType.Integer), ElementType.Integer)
          ),
          CreateTestCase(
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
            new Element(new Token("-", TokenType.MinusSign), ElementType.NegativeOperator),
            new Element(new Token("2", TokenType.Integer), ElementType.Integer)
          ),
          CreateTestCase(
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
            new Element(new Token("+", TokenType.PlusSign), ElementType.PositiveOperator),
            new Element(new Token("2", TokenType.Integer), ElementType.Integer)
          ),
          CreateTestCase(
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new Element(new Token("-", TokenType.MinusSign), ElementType.NegativeOperator),
            new Element(new Token("2", TokenType.Integer), ElementType.Integer)
          ),
          CreateTestCase(
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new Element(new Token("+", TokenType.PlusSign), ElementType.PositiveOperator),
            new Element(new Token("2", TokenType.Integer), ElementType.Integer)
          ),
          CreateTestCase(
            new Element(new Token("-", TokenType.MinusSign), ElementType.NegativeOperator),
            new Element(new Token("(", TokenType.LeftParenthesis), ElementType.LeftParenthesis),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis)
          ),
          CreateTestCase(
            new Element(new Token("+", TokenType.PlusSign), ElementType.PositiveOperator),
            new Element(new Token("(", TokenType.LeftParenthesis), ElementType.LeftParenthesis),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis)
          ),
          CreateTestCase(
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
            new Element(new Token("(", TokenType.LeftParenthesis), ElementType.LeftParenthesis),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis)
          ),
          CreateTestCase(
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new Element(new Token("(", TokenType.LeftParenthesis), ElementType.LeftParenthesis),
            new Element(new Token("2", TokenType.Integer), ElementType.Integer),
            new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis)
          ),
          CreateTestCase(
            new Element(new Token("(", TokenType.LeftParenthesis), ElementType.LeftParenthesis),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis),
            new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer)
          ),
          CreateTestCase(
            new Element(new Token("(", TokenType.LeftParenthesis), ElementType.LeftParenthesis),
            new Element(new Token("2", TokenType.Integer), ElementType.Integer),
            new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis),
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer)
          ),
          CreateTestCase(
            new Element(new Token("(", TokenType.LeftParenthesis), ElementType.LeftParenthesis),
            new Element(new Token("-", TokenType.MinusSign), ElementType.NegativeOperator),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis)
          ),
          CreateTestCase(
            new Element(new Token("(", TokenType.LeftParenthesis), ElementType.LeftParenthesis),
            new Element(new Token("+", TokenType.PlusSign), ElementType.PositiveOperator),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis)
          ),
          CreateTestCase(
            new Element(new Token("-", TokenType.MinusSign), ElementType.NegativeOperator),
            new Element(new Token("+", TokenType.PlusSign), ElementType.PositiveOperator),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer)
          ),
          CreateTestCase(
            new Element(new Token("+", TokenType.PlusSign), ElementType.PositiveOperator),
            new Element(new Token("-", TokenType.MinusSign), ElementType.NegativeOperator),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer)
          ),
          CreateTestCase(
            new Element(new Token("-", TokenType.MinusSign), ElementType.NegativeOperator),
            new Element(new Token("+", TokenType.PlusSign), ElementType.PositiveOperator),
            new Element(new Token("-", TokenType.MinusSign), ElementType.NegativeOperator),
            new Element(new Token("+", TokenType.PlusSign), ElementType.PositiveOperator),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer)
          ),
          CreateTestCase(
            new Element(new Token("+", TokenType.PlusSign), ElementType.PositiveOperator),
            new Element(new Token("-", TokenType.MinusSign), ElementType.NegativeOperator),
            new Element(new Token("+", TokenType.PlusSign), ElementType.PositiveOperator),
            new Element(new Token("-", TokenType.MinusSign), ElementType.NegativeOperator),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer)
          ),
        };
      }
    }
  }
}