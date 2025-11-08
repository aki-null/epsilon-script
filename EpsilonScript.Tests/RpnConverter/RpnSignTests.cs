using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.RpnConverter
{
  public class RpnSignTests : RpnConverterTestBase
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    internal void RpnConverter_SignOperators_ConvertCorrectly(Element[] input, Element[] expected)
    {
      AssertRpnSucceeds(input, expected);
    }

    public static IEnumerable<object[]> CorrectData
    {
      get
      {
        (string text, TokenType token, ElementType element)[] specification =
        {
          ("-", TokenType.MinusSign, ElementType.NegativeOperator),
          ("value", TokenType.Identifier, ElementType.Variable)
        };
        (string text, TokenType token, ElementType element)[] specification1 =
        {
          ("value", TokenType.Identifier, ElementType.Variable),
          ("-", TokenType.MinusSign, ElementType.NegativeOperator)
        };
        yield return new object[]
        {
          ElementFactory.Create(specification),
          ElementFactory.Create(specification1)
        };

        (string text, TokenType token, ElementType element)[] specification2 =
        {
          ("+", TokenType.PlusSign, ElementType.PositiveOperator), ("value", TokenType.Identifier, ElementType.Variable)
        };
        (string text, TokenType token, ElementType element)[] specification3 =
        {
          ("value", TokenType.Identifier, ElementType.Variable), ("+", TokenType.PlusSign, ElementType.PositiveOperator)
        };
        yield return new object[]
        {
          ElementFactory.Create(specification2),
          ElementFactory.Create(specification3)
        };

        (string text, TokenType token, ElementType element)[] specification4 =
        {
          ("1", TokenType.Integer, ElementType.Integer), ("-", TokenType.MinusSign, ElementType.SubtractOperator),
          ("2", TokenType.Integer, ElementType.Integer)
        };
        (string text, TokenType token, ElementType element)[] specification5 =
        {
          ("1", TokenType.Integer, ElementType.Integer), ("2", TokenType.Integer, ElementType.Integer),
          ("-", TokenType.MinusSign, ElementType.SubtractOperator)
        };
        yield return new object[]
        {
          ElementFactory.Create(specification4),
          ElementFactory.Create(specification5)
        };

        (string text, TokenType token, ElementType element)[] specification6 =
        {
          ("1", TokenType.Integer, ElementType.Integer), ("+", TokenType.PlusSign, ElementType.AddOperator),
          ("2", TokenType.Integer, ElementType.Integer)
        };
        (string text, TokenType token, ElementType element)[] specification7 =
        {
          ("1", TokenType.Integer, ElementType.Integer), ("2", TokenType.Integer, ElementType.Integer),
          ("+", TokenType.PlusSign, ElementType.AddOperator)
        };
        yield return new object[]
        {
          ElementFactory.Create(specification6),
          ElementFactory.Create(specification7)
        };

        (string text, TokenType token, ElementType element)[] specification8 =
        {
          ("1", TokenType.Integer, ElementType.Integer), ("-", TokenType.MinusSign, ElementType.SubtractOperator),
          ("-", TokenType.MinusSign, ElementType.NegativeOperator), ("2", TokenType.Integer, ElementType.Integer)
        };
        (string text, TokenType token, ElementType element)[] specification9 =
        {
          ("1", TokenType.Integer, ElementType.Integer), ("2", TokenType.Integer, ElementType.Integer),
          ("-", TokenType.MinusSign, ElementType.NegativeOperator),
          ("-", TokenType.MinusSign, ElementType.SubtractOperator)
        };
        yield return new object[]
        {
          ElementFactory.Create(specification8),
          ElementFactory.Create(specification9)
        };

        (string text, TokenType token, ElementType element)[] specification10 =
        {
          ("1", TokenType.Integer, ElementType.Integer), ("-", TokenType.MinusSign, ElementType.SubtractOperator),
          ("+", TokenType.PlusSign, ElementType.PositiveOperator), ("2", TokenType.Integer, ElementType.Integer)
        };
        (string text, TokenType token, ElementType element)[] specification11 =
        {
          ("1", TokenType.Integer, ElementType.Integer), ("2", TokenType.Integer, ElementType.Integer),
          ("+", TokenType.PlusSign, ElementType.PositiveOperator),
          ("-", TokenType.MinusSign, ElementType.SubtractOperator)
        };
        yield return new object[]
        {
          ElementFactory.Create(specification10),
          ElementFactory.Create(specification11)
        };

        (string text, TokenType token, ElementType element)[] specification12 =
        {
          ("1", TokenType.Integer, ElementType.Integer), ("+", TokenType.PlusSign, ElementType.AddOperator),
          ("-", TokenType.MinusSign, ElementType.NegativeOperator), ("2", TokenType.Integer, ElementType.Integer)
        };
        (string text, TokenType token, ElementType element)[] specification13 =
        {
          ("1", TokenType.Integer, ElementType.Integer), ("2", TokenType.Integer, ElementType.Integer),
          ("-", TokenType.MinusSign, ElementType.NegativeOperator), ("+", TokenType.PlusSign, ElementType.AddOperator)
        };
        yield return new object[]
        {
          ElementFactory.Create(specification12),
          ElementFactory.Create(specification13)
        };

        (string text, TokenType token, ElementType element)[] specification14 =
        {
          ("1", TokenType.Integer, ElementType.Integer), ("+", TokenType.PlusSign, ElementType.AddOperator),
          ("+", TokenType.PlusSign, ElementType.PositiveOperator), ("2", TokenType.Integer, ElementType.Integer)
        };
        (string text, TokenType token, ElementType element)[] specification15 =
        {
          ("1", TokenType.Integer, ElementType.Integer), ("2", TokenType.Integer, ElementType.Integer),
          ("+", TokenType.PlusSign, ElementType.PositiveOperator), ("+", TokenType.PlusSign, ElementType.AddOperator)
        };
        yield return new object[]
        {
          ElementFactory.Create(specification14),
          ElementFactory.Create(specification15)
        };

        (string text, TokenType token, ElementType element)[] specification16 =
        {
          ("-", TokenType.MinusSign, ElementType.NegativeOperator),
          ("(", TokenType.LeftParenthesis, ElementType.LeftParenthesis), ("1", TokenType.Integer, ElementType.Integer),
          (")", TokenType.RightParenthesis, ElementType.RightParenthesis)
        };
        (string text, TokenType token, ElementType element)[] specification17 =
          { ("1", TokenType.Integer, ElementType.Integer), ("-", TokenType.MinusSign, ElementType.NegativeOperator) };
        yield return new object[]
        {
          ElementFactory.Create(specification16),
          ElementFactory.Create(specification17)
        };

        (string text, TokenType token, ElementType element)[] specification18 =
        {
          ("+", TokenType.PlusSign, ElementType.PositiveOperator),
          ("(", TokenType.LeftParenthesis, ElementType.LeftParenthesis), ("1", TokenType.Integer, ElementType.Integer),
          (")", TokenType.RightParenthesis, ElementType.RightParenthesis)
        };
        (string text, TokenType token, ElementType element)[] specification19 =
          { ("1", TokenType.Integer, ElementType.Integer), ("+", TokenType.PlusSign, ElementType.PositiveOperator) };
        yield return new object[]
        {
          ElementFactory.Create(specification18),
          ElementFactory.Create(specification19)
        };

        (string text, TokenType token, ElementType element)[] specification20 =
        {
          ("1", TokenType.Integer, ElementType.Integer), ("-", TokenType.MinusSign, ElementType.SubtractOperator),
          ("(", TokenType.LeftParenthesis, ElementType.LeftParenthesis), ("1", TokenType.Integer, ElementType.Integer),
          (")", TokenType.RightParenthesis, ElementType.RightParenthesis)
        };
        (string text, TokenType token, ElementType element)[] specification21 =
        {
          ("1", TokenType.Integer, ElementType.Integer), ("1", TokenType.Integer, ElementType.Integer),
          ("-", TokenType.MinusSign, ElementType.SubtractOperator)
        };
        yield return new object[]
        {
          ElementFactory.Create(specification20),
          ElementFactory.Create(specification21)
        };

        (string text, TokenType token, ElementType element)[] specification22 =
        {
          ("1", TokenType.Integer, ElementType.Integer), ("+", TokenType.PlusSign, ElementType.AddOperator),
          ("(", TokenType.LeftParenthesis, ElementType.LeftParenthesis), ("2", TokenType.Integer, ElementType.Integer),
          (")", TokenType.RightParenthesis, ElementType.RightParenthesis)
        };
        (string text, TokenType token, ElementType element)[] specification23 =
        {
          ("1", TokenType.Integer, ElementType.Integer), ("2", TokenType.Integer, ElementType.Integer),
          ("+", TokenType.PlusSign, ElementType.AddOperator)
        };
        yield return new object[]
        {
          ElementFactory.Create(specification22),
          ElementFactory.Create(specification23)
        };

        (string text, TokenType token, ElementType element)[] specification24 =
        {
          ("(", TokenType.LeftParenthesis, ElementType.LeftParenthesis), ("1", TokenType.Integer, ElementType.Integer),
          (")", TokenType.RightParenthesis, ElementType.RightParenthesis),
          ("-", TokenType.MinusSign, ElementType.SubtractOperator), ("1", TokenType.Integer, ElementType.Integer)
        };
        (string text, TokenType token, ElementType element)[] specification25 =
        {
          ("1", TokenType.Integer, ElementType.Integer), ("1", TokenType.Integer, ElementType.Integer),
          ("-", TokenType.MinusSign, ElementType.SubtractOperator)
        };
        yield return new object[]
        {
          ElementFactory.Create(specification24),
          ElementFactory.Create(specification25)
        };

        (string text, TokenType token, ElementType element)[] specification26 =
        {
          ("(", TokenType.LeftParenthesis, ElementType.LeftParenthesis), ("2", TokenType.Integer, ElementType.Integer),
          (")", TokenType.RightParenthesis, ElementType.RightParenthesis),
          ("+", TokenType.PlusSign, ElementType.AddOperator), ("1", TokenType.Integer, ElementType.Integer)
        };
        (string text, TokenType token, ElementType element)[] specification27 =
        {
          ("2", TokenType.Integer, ElementType.Integer), ("1", TokenType.Integer, ElementType.Integer),
          ("+", TokenType.PlusSign, ElementType.AddOperator)
        };
        yield return new object[]
        {
          ElementFactory.Create(specification26),
          ElementFactory.Create(specification27)
        };

        (string text, TokenType token, ElementType element)[] specification28 =
        {
          ("(", TokenType.LeftParenthesis, ElementType.LeftParenthesis),
          ("-", TokenType.MinusSign, ElementType.NegativeOperator), ("1", TokenType.Integer, ElementType.Integer),
          (")", TokenType.RightParenthesis, ElementType.RightParenthesis)
        };
        (string text, TokenType token, ElementType element)[] specification29 =
          { ("1", TokenType.Integer, ElementType.Integer), ("-", TokenType.MinusSign, ElementType.NegativeOperator) };
        yield return new object[]
        {
          ElementFactory.Create(specification28),
          ElementFactory.Create(specification29)
        };

        (string text, TokenType token, ElementType element)[] specification30 =
        {
          ("(", TokenType.LeftParenthesis, ElementType.LeftParenthesis),
          ("+", TokenType.PlusSign, ElementType.PositiveOperator), ("1", TokenType.Integer, ElementType.Integer),
          (")", TokenType.RightParenthesis, ElementType.RightParenthesis)
        };
        (string text, TokenType token, ElementType element)[] specification31 =
          { ("1", TokenType.Integer, ElementType.Integer), ("+", TokenType.PlusSign, ElementType.PositiveOperator) };
        yield return new object[]
        {
          ElementFactory.Create(specification30),
          ElementFactory.Create(specification31)
        };

        (string text, TokenType token, ElementType element)[] specification32 =
        {
          ("-", TokenType.MinusSign, ElementType.NegativeOperator),
          ("+", TokenType.PlusSign, ElementType.PositiveOperator), ("1", TokenType.Integer, ElementType.Integer)
        };
        (string text, TokenType token, ElementType element)[] specification33 =
        {
          ("1", TokenType.Integer, ElementType.Integer), ("+", TokenType.PlusSign, ElementType.PositiveOperator),
          ("-", TokenType.MinusSign, ElementType.NegativeOperator)
        };
        yield return new object[]
        {
          ElementFactory.Create(specification32),
          ElementFactory.Create(specification33)
        };

        (string text, TokenType token, ElementType element)[] specification34 =
        {
          ("+", TokenType.PlusSign, ElementType.PositiveOperator),
          ("-", TokenType.MinusSign, ElementType.NegativeOperator), ("1", TokenType.Integer, ElementType.Integer)
        };
        (string text, TokenType token, ElementType element)[] specification35 =
        {
          ("1", TokenType.Integer, ElementType.Integer), ("-", TokenType.MinusSign, ElementType.NegativeOperator),
          ("+", TokenType.PlusSign, ElementType.PositiveOperator)
        };
        yield return new object[]
        {
          ElementFactory.Create(specification34),
          ElementFactory.Create(specification35)
        };
      }
    }
  }
}