using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Intermediate;
using Xunit;

namespace EpsilonScript.Tests
{
  public class AST_Float
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void AST_BuildFloat_Succeeds(Element element, ValueType expectedNodeType, int expectedInt,
      float expectedFloat, bool expectedBool)
    {
      var node = new FloatNode();
      var rpn = new Stack<Node>();
      node.Build(rpn, element, Compiler.Options.None, null,
        null);
      Assert.Equal(expectedNodeType, node.ValueType);
      Assert.Equal(expectedInt, node.IntegerValue);
      Assert.True(Math.IsNearlyEqual(expectedFloat, node.FloatValue));
      Assert.Equal(expectedBool, node.BooleanValue);
    }

    [Theory]
    [InlineData(0.0f)]
    [InlineData(1.0f)]
    [InlineData(1.2f)]
    [InlineData(3.402823E+38)]
    public void AST_CreateFloat_Succeeds(float value)
    {
      var node = new FloatNode(value);
      var expectedInt = (int)value;
      var expectedBool = expectedInt != 0;
      Assert.Equal(ValueType.Float, node.ValueType);
      Assert.Equal(expectedInt, node.IntegerValue);
      Assert.True(Math.IsNearlyEqual(value, node.FloatValue));
      Assert.Equal(expectedBool, node.BooleanValue);
    }

    public static IEnumerable<object[]> CorrectData
    {
      get
      {
        return new[]
        {
          new object[]
          {
            new Element(new Token("0.0", TokenType.Float), ElementType.Float),
            ValueType.Float,
            0,
            0.0f,
            false
          },
          new object[]
          {
            new Element(new Token("1.0", TokenType.Float), ElementType.Float),
            ValueType.Float,
            1,
            1.0f,
            true
          },
          new object[]
          {
            new Element(new Token("1.2", TokenType.Float), ElementType.Float),
            ValueType.Float,
            1,
            1.2f,
            true
          },
          new object[]
          {
            new Element(new Token("1e+5", TokenType.Float), ElementType.Float),
            ValueType.Float,
            100000,
            1.0E+5,
            true
          },
          new object[]
          {
            new Element(new Token("1E+5", TokenType.Float), ElementType.Float),
            ValueType.Float,
            100000,
            1.0E+5,
            true
          },
          new object[]
          {
            new Element(new Token("1.2E+5", TokenType.Float), ElementType.Float),
            ValueType.Float,
            120000,
            1.2E+5,
            true
          },
          new object[]
          {
            new Element(new Token("3.402823E+38", TokenType.Float), ElementType.Float),
            ValueType.Float,
            -2147483648,
            3.402823E+38f,
            true
          },
        };
      }
    }
  }
}