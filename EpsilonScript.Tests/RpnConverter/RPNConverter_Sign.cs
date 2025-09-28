using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.RpnConverter
{
  public class RPNConverter_Sign : RpnConverterTestBase
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void RPNConverter_ParsesSign_Correctly(Element[] input, Element[] expected)
    {
      AssertRpnSucceeds(input, expected);
    }

    public static IEnumerable<object[]> CorrectData
    {
      get
      {
        yield return new object[]
        {
          Elements(
            ("-", TokenType.MinusSign, ElementType.NegativeOperator),
            ("value", TokenType.Identifier, ElementType.Variable)
          ),
          Elements(
            ("value", TokenType.Identifier, ElementType.Variable),
            ("-", TokenType.MinusSign, ElementType.NegativeOperator)
          )
        };

        yield return new object[]
        {
          Elements(
            ("+", TokenType.PlusSign, ElementType.PositiveOperator),
            ("value", TokenType.Identifier, ElementType.Variable)
          ),
          Elements(
            ("value", TokenType.Identifier, ElementType.Variable),
            ("+", TokenType.PlusSign, ElementType.PositiveOperator)
          )
        };

        yield return new object[]
        {
          Elements(
            ("1", TokenType.Integer, ElementType.Integer),
            ("-", TokenType.MinusSign, ElementType.SubtractOperator),
            ("2", TokenType.Integer, ElementType.Integer)
          ),
          Elements(
            ("1", TokenType.Integer, ElementType.Integer),
            ("2", TokenType.Integer, ElementType.Integer),
            ("-", TokenType.MinusSign, ElementType.SubtractOperator)
          )
        };

        yield return new object[]
        {
          Elements(
            ("1", TokenType.Integer, ElementType.Integer),
            ("+", TokenType.PlusSign, ElementType.AddOperator),
            ("2", TokenType.Integer, ElementType.Integer)
          ),
          Elements(
            ("1", TokenType.Integer, ElementType.Integer),
            ("2", TokenType.Integer, ElementType.Integer),
            ("+", TokenType.PlusSign, ElementType.AddOperator)
          )
        };

        yield return new object[]
        {
          Elements(
            ("1", TokenType.Integer, ElementType.Integer),
            ("-", TokenType.MinusSign, ElementType.SubtractOperator),
            ("-", TokenType.MinusSign, ElementType.NegativeOperator),
            ("2", TokenType.Integer, ElementType.Integer)
          ),
          Elements(
            ("1", TokenType.Integer, ElementType.Integer),
            ("2", TokenType.Integer, ElementType.Integer),
            ("-", TokenType.MinusSign, ElementType.NegativeOperator),
            ("-", TokenType.MinusSign, ElementType.SubtractOperator)
          )
        };

        yield return new object[]
        {
          Elements(
            ("1", TokenType.Integer, ElementType.Integer),
            ("-", TokenType.MinusSign, ElementType.SubtractOperator),
            ("+", TokenType.PlusSign, ElementType.PositiveOperator),
            ("2", TokenType.Integer, ElementType.Integer)
          ),
          Elements(
            ("1", TokenType.Integer, ElementType.Integer),
            ("2", TokenType.Integer, ElementType.Integer),
            ("+", TokenType.PlusSign, ElementType.PositiveOperator),
            ("-", TokenType.MinusSign, ElementType.SubtractOperator)
          )
        };

        yield return new object[]
        {
          Elements(
            ("1", TokenType.Integer, ElementType.Integer),
            ("+", TokenType.PlusSign, ElementType.AddOperator),
            ("-", TokenType.MinusSign, ElementType.NegativeOperator),
            ("2", TokenType.Integer, ElementType.Integer)
          ),
          Elements(
            ("1", TokenType.Integer, ElementType.Integer),
            ("2", TokenType.Integer, ElementType.Integer),
            ("-", TokenType.MinusSign, ElementType.NegativeOperator),
            ("+", TokenType.PlusSign, ElementType.AddOperator)
          )
        };

        yield return new object[]
        {
          Elements(
            ("1", TokenType.Integer, ElementType.Integer),
            ("+", TokenType.PlusSign, ElementType.AddOperator),
            ("+", TokenType.PlusSign, ElementType.PositiveOperator),
            ("2", TokenType.Integer, ElementType.Integer)
          ),
          Elements(
            ("1", TokenType.Integer, ElementType.Integer),
            ("2", TokenType.Integer, ElementType.Integer),
            ("+", TokenType.PlusSign, ElementType.PositiveOperator),
            ("+", TokenType.PlusSign, ElementType.AddOperator)
          )
        };

        yield return new object[]
        {
          Elements(
            ("-", TokenType.MinusSign, ElementType.NegativeOperator),
            ("(", TokenType.LeftParenthesis, ElementType.LeftParenthesis),
            ("1", TokenType.Integer, ElementType.Integer),
            (")", TokenType.RightParenthesis, ElementType.RightParenthesis)
          ),
          Elements(
            ("1", TokenType.Integer, ElementType.Integer),
            ("-", TokenType.MinusSign, ElementType.NegativeOperator)
          )
        };

        yield return new object[]
        {
          Elements(
            ("+", TokenType.PlusSign, ElementType.PositiveOperator),
            ("(", TokenType.LeftParenthesis, ElementType.LeftParenthesis),
            ("1", TokenType.Integer, ElementType.Integer),
            (")", TokenType.RightParenthesis, ElementType.RightParenthesis)
          ),
          Elements(
            ("1", TokenType.Integer, ElementType.Integer),
            ("+", TokenType.PlusSign, ElementType.PositiveOperator)
          )
        };

        yield return new object[]
        {
          Elements(
            ("1", TokenType.Integer, ElementType.Integer),
            ("-", TokenType.MinusSign, ElementType.SubtractOperator),
            ("(", TokenType.LeftParenthesis, ElementType.LeftParenthesis),
            ("1", TokenType.Integer, ElementType.Integer),
            (")", TokenType.RightParenthesis, ElementType.RightParenthesis)
          ),
          Elements(
            ("1", TokenType.Integer, ElementType.Integer),
            ("1", TokenType.Integer, ElementType.Integer),
            ("-", TokenType.MinusSign, ElementType.SubtractOperator)
          )
        };

        yield return new object[]
        {
          Elements(
            ("1", TokenType.Integer, ElementType.Integer),
            ("+", TokenType.PlusSign, ElementType.AddOperator),
            ("(", TokenType.LeftParenthesis, ElementType.LeftParenthesis),
            ("2", TokenType.Integer, ElementType.Integer),
            (")", TokenType.RightParenthesis, ElementType.RightParenthesis)
          ),
          Elements(
            ("1", TokenType.Integer, ElementType.Integer),
            ("2", TokenType.Integer, ElementType.Integer),
            ("+", TokenType.PlusSign, ElementType.AddOperator)
          )
        };

        yield return new object[]
        {
          Elements(
            ("(", TokenType.LeftParenthesis, ElementType.LeftParenthesis),
            ("1", TokenType.Integer, ElementType.Integer),
            (")", TokenType.RightParenthesis, ElementType.RightParenthesis),
            ("-", TokenType.MinusSign, ElementType.SubtractOperator),
            ("1", TokenType.Integer, ElementType.Integer)
          ),
          Elements(
            ("1", TokenType.Integer, ElementType.Integer),
            ("1", TokenType.Integer, ElementType.Integer),
            ("-", TokenType.MinusSign, ElementType.SubtractOperator)
          )
        };

        yield return new object[]
        {
          Elements(
            ("(", TokenType.LeftParenthesis, ElementType.LeftParenthesis),
            ("2", TokenType.Integer, ElementType.Integer),
            (")", TokenType.RightParenthesis, ElementType.RightParenthesis),
            ("+", TokenType.PlusSign, ElementType.AddOperator),
            ("1", TokenType.Integer, ElementType.Integer)
          ),
          Elements(
            ("2", TokenType.Integer, ElementType.Integer),
            ("1", TokenType.Integer, ElementType.Integer),
            ("+", TokenType.PlusSign, ElementType.AddOperator)
          )
        };

        yield return new object[]
        {
          Elements(
            ("(", TokenType.LeftParenthesis, ElementType.LeftParenthesis),
            ("-", TokenType.MinusSign, ElementType.NegativeOperator),
            ("1", TokenType.Integer, ElementType.Integer),
            (")", TokenType.RightParenthesis, ElementType.RightParenthesis)
          ),
          Elements(
            ("1", TokenType.Integer, ElementType.Integer),
            ("-", TokenType.MinusSign, ElementType.NegativeOperator)
          )
        };

        yield return new object[]
        {
          Elements(
            ("(", TokenType.LeftParenthesis, ElementType.LeftParenthesis),
            ("+", TokenType.PlusSign, ElementType.PositiveOperator),
            ("1", TokenType.Integer, ElementType.Integer),
            (")", TokenType.RightParenthesis, ElementType.RightParenthesis)
          ),
          Elements(
            ("1", TokenType.Integer, ElementType.Integer),
            ("+", TokenType.PlusSign, ElementType.PositiveOperator)
          )
        };

        yield return new object[]
        {
          Elements(
            ("-", TokenType.MinusSign, ElementType.NegativeOperator),
            ("+", TokenType.PlusSign, ElementType.PositiveOperator),
            ("1", TokenType.Integer, ElementType.Integer)
          ),
          Elements(
            ("1", TokenType.Integer, ElementType.Integer),
            ("+", TokenType.PlusSign, ElementType.PositiveOperator),
            ("-", TokenType.MinusSign, ElementType.NegativeOperator)
          )
        };

        yield return new object[]
        {
          Elements(
            ("+", TokenType.PlusSign, ElementType.PositiveOperator),
            ("-", TokenType.MinusSign, ElementType.NegativeOperator),
            ("1", TokenType.Integer, ElementType.Integer)
          ),
          Elements(
            ("1", TokenType.Integer, ElementType.Integer),
            ("-", TokenType.MinusSign, ElementType.NegativeOperator),
            ("+", TokenType.PlusSign, ElementType.PositiveOperator)
          )
        };
      }
    }
  }
}
