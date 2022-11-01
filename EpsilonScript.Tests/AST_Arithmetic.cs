using System;
using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Intermediate;
using Xunit;
using ValueType = EpsilonScript.AST.ValueType;

namespace EpsilonScript.Tests
{
  public class AST_Arithmetic
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void AST_Arithmetic_Succeeds(Element element, Node leftNode, Node rightNode, ValueType expectedNodeType,
      int expectedInt, float expectedFloat, bool expectedBool, string expectedString)
    {
      var node = new ArithmeticNode();
      var rpn = new Stack<Node>();
      rpn.Push(leftNode);
      rpn.Push(rightNode);
      node.Build(rpn, element, Compiler.Options.None, null, null);
      node.Execute(null);
      Assert.Equal(expectedNodeType, node.ValueType);
      Assert.Equal(expectedInt, node.IntegerValue);
      Assert.True(Math.IsNearlyEqual(expectedFloat, node.FloatValue));
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
            new FakeIntegerNode(65536 + 1),
            new FakeIntegerNode(2147418111),
            ValueType.Integer,
            -2147483648,
            -2147483648.0f,
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
            new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
            new FakeIntegerNode(-65537 - 1),
            new FakeIntegerNode(2147418111),
            ValueType.Integer,
            2147483647,
            2147483647.0f,
            true,
            null
          },
          // TODO MultiplyOperator
          // TODO DivideOperator
          // TODO ModuloOperator
        };
      }
    }
  }
}