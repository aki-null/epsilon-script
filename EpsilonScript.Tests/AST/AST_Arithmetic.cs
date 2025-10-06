using System;
using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;
using EpsilonScript.Tests.TestInfrastructure.Fakes;
using ValueType = EpsilonScript.AST.ValueType;

namespace EpsilonScript.Tests.AST
{
  [Trait("Category", "Unit")]
  [Trait("Component", "AST")]
  public class AST_Arithmetic : AstTestBase
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void AST_Arithmetic_Succeeds(Element element, Node leftNode, Node rightNode, ValueType expectedNodeType,
      int expectedInt, float expectedFloat, bool expectedBool, string expectedString)
    {
      var node = new ArithmeticNode();
      var rpn = CreateStack(leftNode, rightNode);
      node.Build(rpn, element, Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer,
        Compiler.FloatPrecision.Float);
      node.Execute(null);
      Assert.Equal(expectedNodeType, node.ValueType);
      Assert.Equal(expectedInt, node.IntegerValue);
      Assert.True(EpsilonScript.Math.IsNearlyEqual(expectedFloat, node.FloatValue));
      Assert.Equal(expectedBool, node.BooleanValue);
      Assert.Equal(expectedString, node.StringValue);
    }

    public static IEnumerable<object[]> CorrectData
    {
      get
      {
        return new[]
        {
          new object[]
          {
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new FakeIntegerNode(0),
            new FakeIntegerNode(0),
            ValueType.Integer,
            0,
            0.0f,
            false,
            null
          },
          new object[]
          {
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new FakeIntegerNode(256),
            new FakeIntegerNode(128),
            ValueType.Integer,
            384,
            384.0f,
            true,
            null
          },
          new object[]
          {
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new FakeIntegerNode(256),
            new FakeIntegerNode(-128),
            ValueType.Integer,
            128,
            128.0f,
            true,
            null
          },
          new object[]
          {
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new FakeFloatNode(0),
            new FakeFloatNode(0),
            ValueType.Float,
            0,
            0.0f,
            false,
            null
          },
          new object[]
          {
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new FakeIntegerNode(0),
            new FakeFloatNode(0),
            ValueType.Float,
            0,
            0.0f,
            false,
            null
          },
          new object[]
          {
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new FakeFloatNode(0),
            new FakeIntegerNode(0),
            ValueType.Float,
            0,
            0.0f,
            false,
            null
          },
          new object[]
          {
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new FakeIntegerNode(1),
            new FakeIntegerNode(5),
            ValueType.Integer,
            6,
            6.0f,
            true,
            null
          },
          new object[]
          {
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new FakeIntegerNode(65536),
            new FakeIntegerNode(2147418111),
            ValueType.Integer,
            2147483647,
            2147483647.0f,
            true,
            null
          },
          new object[]
          {
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new FakeStringNode(""),
            new FakeStringNode(""),
            ValueType.String,
            0,
            0.0f,
            false,
            ""
          },
          new object[]
          {
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new FakeStringNode("Hello "),
            new FakeStringNode("World"),
            ValueType.String,
            0,
            0.0f,
            false,
            "Hello World"
          },
          new object[]
          {
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new FakeStringNode("Hello"),
            new FakeStringNode(""),
            ValueType.String,
            0,
            0.0f,
            false,
            "Hello"
          },
          new object[]
          {
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new FakeStringNode(""),
            new FakeStringNode("Hello"),
            ValueType.String,
            0,
            0.0f,
            false,
            "Hello"
          },
          new object[]
          {
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new FakeStringNode("Life "),
            new FakeIntegerNode(42),
            ValueType.String,
            0,
            0.0f,
            false,
            "Life 42"
          },
          new object[]
          {
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new FakeStringNode("Life "),
            new FakeIntegerNode(-42),
            ValueType.String,
            0,
            0.0f,
            false,
            "Life -42"
          },
          new object[]
          {
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new FakeStringNode("Life "),
            new FakeFloatNode(-42.42f),
            ValueType.String,
            0,
            0.0f,
            false,
            "Life -42.42"
          },
          new object[]
          {
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new FakeStringNode("Pi "),
            new FakeFloatNode(3.14159265359f),
            ValueType.String,
            0,
            0.0f,
            false,
            "Pi 3.1415927"
          },
          new object[]
          {
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new FakeStringNode("-Pi "),
            new FakeFloatNode(-3.14159265359f),
            ValueType.String,
            0,
            0.0f,
            false,
            "-Pi -3.1415927"
          },
          new object[]
          {
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new FakeStringNode("true "),
            new FakeBooleanNode(true),
            ValueType.String,
            0,
            0.0f,
            false,
            "true true"
          },
          new object[]
          {
            new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
            new FakeStringNode("false "),
            new FakeBooleanNode(false),
            ValueType.String,
            0,
            0.0f,
            false,
            "false false"
          },
          new object[]
          {
            new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
            new FakeIntegerNode(0),
            new FakeIntegerNode(0),
            ValueType.Integer,
            0,
            0.0f,
            false,
            null
          },
          new object[]
          {
            new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
            new FakeFloatNode(0),
            new FakeFloatNode(0),
            ValueType.Float,
            0,
            0.0f,
            false,
            null
          },
          new object[]
          {
            new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
            new FakeIntegerNode(0),
            new FakeFloatNode(0),
            ValueType.Float,
            0,
            0.0f,
            false,
            null
          },
          new object[]
          {
            new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
            new FakeFloatNode(0),
            new FakeIntegerNode(0),
            ValueType.Float,
            0,
            0.0f,
            false,
            null
          },
          new object[]
          {
            new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
            new FakeIntegerNode(1),
            new FakeIntegerNode(5),
            ValueType.Integer,
            -4,
            -4.0f,
            true,
            null
          },
          new object[]
          {
            new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
            new FakeIntegerNode(1),
            new FakeIntegerNode(-5),
            ValueType.Integer,
            6,
            6.0f,
            true,
            null
          },
          new object[]
          {
            new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
            new FakeIntegerNode(-65537),
            new FakeIntegerNode(2147418111),
            ValueType.Integer,
            -2147483648,
            -2147483648.0f,
            true,
            null
          },
          new object[]
          {
            new Element(new Token("*", TokenType.MultiplyOperator), ElementType.MultiplyOperator),
            new FakeIntegerNode(0),
            new FakeIntegerNode(0),
            ValueType.Integer,
            0,
            0.0f,
            false,
            null
          },
          new object[]
          {
            new Element(new Token("*", TokenType.MultiplyOperator), ElementType.MultiplyOperator),
            new FakeIntegerNode(256),
            new FakeIntegerNode(128),
            ValueType.Integer,
            32768,
            32768.0f,
            true,
            null
          },
          new object[]
          {
            new Element(new Token("*", TokenType.MultiplyOperator), ElementType.MultiplyOperator),
            new FakeFloatNode(0),
            new FakeFloatNode(0),
            ValueType.Float,
            0,
            0.0f,
            false,
            null
          },
          new object[]
          {
            new Element(new Token("*", TokenType.MultiplyOperator), ElementType.MultiplyOperator),
            new FakeIntegerNode(0),
            new FakeFloatNode(0),
            ValueType.Float,
            0,
            0.0f,
            false,
            null
          },
          new object[]
          {
            new Element(new Token("*", TokenType.MultiplyOperator), ElementType.MultiplyOperator),
            new FakeFloatNode(0),
            new FakeIntegerNode(0),
            ValueType.Float,
            0,
            0.0f,
            false,
            null
          },
          new object[]
          {
            new Element(new Token("*", TokenType.MultiplyOperator), ElementType.MultiplyOperator),
            new FakeIntegerNode(2),
            new FakeIntegerNode(-5),
            ValueType.Integer,
            -10,
            -10.0f,
            true,
            null
          },
          new object[]
          {
            new Element(new Token("/", TokenType.DivideOperator), ElementType.DivideOperator),
            new FakeIntegerNode(6),
            new FakeIntegerNode(2),
            ValueType.Integer,
            3,
            3.0f,
            true,
            null
          },
          new object[]
          {
            new Element(new Token("/", TokenType.DivideOperator), ElementType.DivideOperator),
            new FakeIntegerNode(7),
            new FakeIntegerNode(2),
            ValueType.Integer,
            3,
            3.0f,
            true,
            null
          },
          new object[]
          {
            new Element(new Token("/", TokenType.DivideOperator), ElementType.DivideOperator),
            new FakeFloatNode(6.0f),
            new FakeFloatNode(2.0f),
            ValueType.Float,
            3,
            3.0f,
            true,
            null
          },
          new object[]
          {
            new Element(new Token("/", TokenType.DivideOperator), ElementType.DivideOperator),
            new FakeFloatNode(7.0f),
            new FakeFloatNode(2.0f),
            ValueType.Float,
            3,
            3.5f,
            true,
            null
          },
          new object[]
          {
            new Element(new Token("/", TokenType.DivideOperator), ElementType.DivideOperator),
            new FakeIntegerNode(7),
            new FakeFloatNode(2.0f),
            ValueType.Float,
            3,
            3.5f,
            true,
            null
          },
          new object[]
          {
            new Element(new Token("/", TokenType.DivideOperator), ElementType.DivideOperator),
            new FakeFloatNode(7.0f),
            new FakeIntegerNode(2),
            ValueType.Float,
            3,
            3.5f,
            true,
            null
          },
          new object[]
          {
            new Element(new Token("/", TokenType.DivideOperator), ElementType.DivideOperator),
            new FakeIntegerNode(0),
            new FakeIntegerNode(5),
            ValueType.Integer,
            0,
            0.0f,
            false,
            null
          },
          new object[]
          {
            new Element(new Token("/", TokenType.DivideOperator), ElementType.DivideOperator),
            new FakeFloatNode(0.0f),
            new FakeFloatNode(5.0f),
            ValueType.Float,
            0,
            0.0f,
            false,
            null
          },
          new object[]
          {
            new Element(new Token("%", TokenType.ModuloOperator), ElementType.ModuloOperator),
            new FakeIntegerNode(7),
            new FakeIntegerNode(2),
            ValueType.Integer,
            1,
            1.0f,
            true,
            null
          },
          new object[]
          {
            new Element(new Token("%", TokenType.ModuloOperator), ElementType.ModuloOperator),
            new FakeIntegerNode(6),
            new FakeIntegerNode(2),
            ValueType.Integer,
            0,
            0.0f,
            false,
            null
          },
          new object[]
          {
            new Element(new Token("%", TokenType.ModuloOperator), ElementType.ModuloOperator),
            new FakeFloatNode(7.5f),
            new FakeFloatNode(2.0f),
            ValueType.Float,
            1,
            1.5f,
            true,
            null
          },
          new object[]
          {
            new Element(new Token("%", TokenType.ModuloOperator), ElementType.ModuloOperator),
            new FakeFloatNode(6.0f),
            new FakeFloatNode(2.0f),
            ValueType.Float,
            0,
            0.0f,
            false,
            null
          },
          new object[]
          {
            new Element(new Token("%", TokenType.ModuloOperator), ElementType.ModuloOperator),
            new FakeIntegerNode(7),
            new FakeFloatNode(2.0f),
            ValueType.Float,
            1,
            1.0f,
            true,
            null
          },
          new object[]
          {
            new Element(new Token("%", TokenType.ModuloOperator), ElementType.ModuloOperator),
            new FakeFloatNode(7.0f),
            new FakeIntegerNode(2),
            ValueType.Float,
            1,
            1.0f,
            true,
            null
          },
          new object[]
          {
            new Element(new Token("%", TokenType.ModuloOperator), ElementType.ModuloOperator),
            new FakeIntegerNode(0),
            new FakeIntegerNode(5),
            ValueType.Integer,
            0,
            0.0f,
            false,
            null
          },
          new object[]
          {
            new Element(new Token("%", TokenType.ModuloOperator), ElementType.ModuloOperator),
            new FakeFloatNode(0.0f),
            new FakeFloatNode(5.0f),
            ValueType.Float,
            0,
            0.0f,
            false,
            null
          }
        };
      }
    }

    [Theory]
    [InlineData(ElementType.DivideOperator, "/")]
    [InlineData(ElementType.ModuloOperator, "%")]
    public void AST_Arithmetic_IntegerByZero_ThrowsDivideByZeroException(ElementType operatorType,
      string operatorSymbol)
    {
      var node = new ArithmeticNode();
      var rpn = CreateStack(new FakeIntegerNode(5), new FakeIntegerNode(0));
      var element = new Element(new Token(operatorSymbol, GetTokenType(operatorType)), operatorType);

      node.Build(rpn, element, Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer,
        Compiler.FloatPrecision.Float);

      Assert.Throws<DivideByZeroException>(() => node.Execute(null));
    }

    [Fact]
    public void AST_Arithmetic_FloatDivisionByZero_ThrowsDivideByZeroException()
    {
      var node = new ArithmeticNode();
      var rpn = CreateStack(new FakeFloatNode(5.0f), new FakeFloatNode(0.0f));
      var element = new Element(new Token("/", TokenType.DivideOperator), ElementType.DivideOperator);

      node.Build(rpn, element, Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer,
        Compiler.FloatPrecision.Float);

      ErrorTestHelper.ExecuteNodeExpectingError<DivideByZeroException>(node);
    }

    [Fact]
    public void AST_Arithmetic_FloatModuloByZero_ThrowsDivideByZeroException()
    {
      var node = new ArithmeticNode();
      var rpn = CreateStack(new FakeFloatNode(5.0f), new FakeFloatNode(0.0f));
      var element = new Element(new Token("%", TokenType.ModuloOperator), ElementType.ModuloOperator);

      node.Build(rpn, element, Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer,
        Compiler.FloatPrecision.Float);

      Assert.Throws<DivideByZeroException>(() => node.Execute(null));
    }

    [Theory]
    [InlineData(ElementType.DivideOperator, "/")]
    [InlineData(ElementType.ModuloOperator, "%")]
    public void AST_Arithmetic_MixedByZero_ThrowsDivideByZeroException(ElementType operatorType, string operatorSymbol)
    {
      var node = new ArithmeticNode();
      var rpn = CreateStack(new FakeIntegerNode(5), new FakeFloatNode(0.0f));
      var element = new Element(new Token(operatorSymbol, GetTokenType(operatorType)), operatorType);

      node.Build(rpn, element, Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer,
        Compiler.FloatPrecision.Float);

      Assert.Throws<DivideByZeroException>(() => node.Execute(null));
    }

    private static TokenType GetTokenType(ElementType operatorType)
    {
      return operatorType switch
      {
        ElementType.DivideOperator => TokenType.DivideOperator,
        ElementType.ModuloOperator => TokenType.ModuloOperator,
        _ => throw new ArgumentOutOfRangeException(nameof(operatorType))
      };
    }
  }
}