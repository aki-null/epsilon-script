using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;

namespace EpsilonScript.Tests
{
  public class TokenParser_Sign : TokenParser_Base
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void RPNConverter_ParsesSign_Correctly(Token[] input, Element[] expected)
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
            new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator)
          ),
          CreateTestData(
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator)
          ),
          CreateTestData(
            new Element(new Token("-", TokenType.MinusSign), ElementType.NegativeOperator),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer)
          ),
          CreateTestData(
            new Element(new Token("+", TokenType.PlusSign), ElementType.PositiveOperator),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer)
          ),
          CreateTestData(
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
            new Element(new Token("2", TokenType.Integer), ElementType.Integer)
          ),
          CreateTestData(
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new Element(new Token("2", TokenType.Integer), ElementType.Integer)
          ),
          CreateTestData(
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
            new Element(new Token("-", TokenType.MinusSign), ElementType.NegativeOperator),
            new Element(new Token("2", TokenType.Integer), ElementType.Integer)
          ),
          CreateTestData(
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
            new Element(new Token("+", TokenType.PlusSign), ElementType.PositiveOperator),
            new Element(new Token("2", TokenType.Integer), ElementType.Integer)
          ),
          CreateTestData(
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new Element(new Token("-", TokenType.MinusSign), ElementType.NegativeOperator),
            new Element(new Token("2", TokenType.Integer), ElementType.Integer)
          ),
          CreateTestData(
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new Element(new Token("+", TokenType.PlusSign), ElementType.PositiveOperator),
            new Element(new Token("2", TokenType.Integer), ElementType.Integer)
          ),
          CreateTestData(
            new Element(new Token("-", TokenType.MinusSign), ElementType.NegativeOperator),
            new Element(new Token("(", TokenType.LeftParenthesis), ElementType.LeftParenthesis),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis)
          ),
          CreateTestData(
            new Element(new Token("+", TokenType.PlusSign), ElementType.PositiveOperator),
            new Element(new Token("(", TokenType.LeftParenthesis), ElementType.LeftParenthesis),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis)
          ),
          CreateTestData(
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
            new Element(new Token("(", TokenType.LeftParenthesis), ElementType.LeftParenthesis),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis)
          ),
          CreateTestData(
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new Element(new Token("(", TokenType.LeftParenthesis), ElementType.LeftParenthesis),
            new Element(new Token("2", TokenType.Integer), ElementType.Integer),
            new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis)
          ),
          CreateTestData(
            new Element(new Token("(", TokenType.LeftParenthesis), ElementType.LeftParenthesis),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis),
            new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer)
          ),
          CreateTestData(
            new Element(new Token("(", TokenType.LeftParenthesis), ElementType.LeftParenthesis),
            new Element(new Token("2", TokenType.Integer), ElementType.Integer),
            new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis),
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer)
          ),
          CreateTestData(
            new Element(new Token("(", TokenType.LeftParenthesis), ElementType.LeftParenthesis),
            new Element(new Token("-", TokenType.MinusSign), ElementType.NegativeOperator),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis)
          ),
          CreateTestData(
            new Element(new Token("(", TokenType.LeftParenthesis), ElementType.LeftParenthesis),
            new Element(new Token("+", TokenType.PlusSign), ElementType.PositiveOperator),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer),
            new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis)
          ),
          CreateTestData(
            new Element(new Token("-", TokenType.MinusSign), ElementType.NegativeOperator),
            new Element(new Token("+", TokenType.PlusSign), ElementType.PositiveOperator),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer)
          ),
          CreateTestData(
            new Element(new Token("+", TokenType.PlusSign), ElementType.PositiveOperator),
            new Element(new Token("-", TokenType.MinusSign), ElementType.NegativeOperator),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer)
          ),
          CreateTestData(
            new Element(new Token("-", TokenType.MinusSign), ElementType.NegativeOperator),
            new Element(new Token("+", TokenType.PlusSign), ElementType.PositiveOperator),
            new Element(new Token("-", TokenType.MinusSign), ElementType.NegativeOperator),
            new Element(new Token("+", TokenType.PlusSign), ElementType.PositiveOperator),
            new Element(new Token("1", TokenType.Integer), ElementType.Integer)
          ),
          CreateTestData(
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