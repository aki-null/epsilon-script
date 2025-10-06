using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.AST
{
  [Trait("Category", "Unit")]
  [Trait("Component", "AST")]
  public class AST_Float : AstTestBase
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void AST_FloatNode_WithValidElement_CreatesCorrectValue(Element element, ValueType expectedNodeType,
      int expectedInt,
      float expectedFloat, bool expectedBool)
    {
      var node = new FloatNode();
      var rpn = CreateStack();
      node.Build(rpn, element, Compiler.Options.None, null,
        null, Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      Assert.Equal(expectedNodeType, node.ValueType);
      Assert.Equal(expectedInt, node.IntegerValue);
      Assert.True(EpsilonScript.Math.IsNearlyEqual(expectedFloat, node.FloatValue));
      Assert.Equal(expectedBool, node.BooleanValue);
    }

    [Theory]
    [InlineData(0.0f)]
    [InlineData(1.0f)]
    [InlineData(1.2f)]
    public void AST_FloatNode_WithConstructorValue_CreatesCorrectNode(float value)
    {
      var node = new FloatNode(value);
      var expectedInt = (int)value;
      var expectedBool = value != 0.0f && !float.IsInfinity(value) && !float.IsNaN(value);
      Assert.Equal(ValueType.Float, node.ValueType);
      Assert.Equal(expectedInt, node.IntegerValue);
      Assert.True(EpsilonScript.Math.IsNearlyEqual(value, node.FloatValue));
      Assert.Equal(expectedBool, node.BooleanValue);
    }

    [Theory]
    [InlineData(float.MaxValue, int.MaxValue)]
    [InlineData(-float.MaxValue, int.MinValue)]
    [InlineData(float.PositiveInfinity, 0)]
    [InlineData(float.NegativeInfinity, 0)]
    [InlineData(float.NaN, 0)]
    public void AST_FloatNode_WithEdgeValues_HasSafeIntegerConversion(float inputValue, int expectedInt)
    {
      // Test safe float-to-int conversion behavior
      // Values outside int32 range are safely handled without overflow
      var node = new FloatNode(inputValue);
      Assert.Equal(ValueType.Float, node.ValueType);
      Assert.Equal(inputValue, node.FloatValue);
      Assert.Equal(expectedInt, node.IntegerValue);
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
        };
      }
    }
  }
}