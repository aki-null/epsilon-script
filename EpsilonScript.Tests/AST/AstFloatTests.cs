using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.AST
{
  [Trait("Category", "Unit")]
  [Trait("Component", "AST")]
  public class AstFloatTests : AstTestBase
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    internal void AST_FloatNode_WithValidElement_CreatesCorrectValue(Element element, ExtendedType expectedNodeType,
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
    internal void AST_FloatNode_WithConstructorValue_CreatesCorrectNode(float value)
    {
      var node = new FloatNode(value);
      var expectedInt = (int)value;
      var expectedBool = value != 0.0f && !float.IsInfinity(value) && !float.IsNaN(value);
      Assert.Equal(ExtendedType.Float, node.ValueType);
      Assert.Equal(expectedInt, node.IntegerValue);
      Assert.True(EpsilonScript.Math.IsNearlyEqual(value, node.FloatValue));
      Assert.Equal(expectedBool, node.BooleanValue);
    }

    [Theory]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    [InlineData(float.NaN)]
    internal void AST_FloatNode_WithSpecialValues_HasSafeIntegerConversion(float inputValue)
    {
      // Test special float values convert to 0
      var node = new FloatNode(inputValue);
      Assert.Equal(ExtendedType.Float, node.ValueType);
      Assert.Equal(inputValue, node.FloatValue);
      Assert.Equal(0, node.IntegerValue);
    }

    [Theory]
    [InlineData(float.MaxValue)]
    [InlineData(-float.MaxValue)]
    internal void AST_FloatNode_WithOverflowValues_UncheckedCast(float inputValue)
    {
      // Test unchecked cast behavior for out-of-range values
      // Float stored as double, then cast to int - matches runtime behavior
      var node = new FloatNode(inputValue);
      Assert.Equal(ExtendedType.Float, node.ValueType);
      Assert.Equal(inputValue, node.FloatValue);

      // Compute expected value using same conversion path as Node.cs
      double d = inputValue;
      var expectedInt = (int)d;
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
            ExtendedType.Float,
            0,
            0.0f,
            false
          },
          new object[]
          {
            new Element(new Token("1.0", TokenType.Float), ElementType.Float),
            ExtendedType.Float,
            1,
            1.0f,
            true
          },
          new object[]
          {
            new Element(new Token("1.2", TokenType.Float), ElementType.Float),
            ExtendedType.Float,
            1,
            1.2f,
            true
          },
          new object[]
          {
            new Element(new Token("1e+5", TokenType.Float), ElementType.Float),
            ExtendedType.Float,
            100000,
            1.0E+5,
            true
          },
          new object[]
          {
            new Element(new Token("1E+5", TokenType.Float), ElementType.Float),
            ExtendedType.Float,
            100000,
            1.0E+5,
            true
          },
          new object[]
          {
            new Element(new Token("1.2E+5", TokenType.Float), ElementType.Float),
            ExtendedType.Float,
            120000,
            1.2E+5,
            true
          },
        };
      }
    }
  }
}