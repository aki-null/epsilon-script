using System.Collections.Generic;
using EpsilonScript.AST.Literal;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.AST
{
  [Trait("Category", "Unit")]
  [Trait("Component", "AST")]
  public class AstPrecisionTests : AstTestBase
  {
    #region Integer Precision Tests

    [Theory]
    [InlineData("0", 0L)]
    [InlineData("1", 1L)]
    [InlineData("2147483647", 2147483647L)] // int.MaxValue
    [InlineData("2147483648", 2147483648L)] // int.MaxValue + 1
    [InlineData("9223372036854775807", 9223372036854775807L)] // long.MaxValue
    [InlineData("-2147483648", -2147483648L)] // int.MinValue
    [InlineData("-2147483649", -2147483649L)] // int.MinValue - 1
    [InlineData("-9223372036854775808", -9223372036854775808L)] // long.MinValue
    public void IntegerNode_WithLongPrecision_StoresLongValue(string literal, long expectedValue)
    {
      var node = new IntegerNode();
      var rpn = CreateStack();
      var element = new Element(new Token(literal, TokenType.Integer), ElementType.Integer);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float, null), Compiler.Options.None,
        null);

      Assert.Equal(ExtendedType.Long, node.ValueType);
      Assert.Equal(expectedValue, node.LongValue);
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("1", 1)]
    [InlineData("2147483647", int.MaxValue)]
    [InlineData("-2147483648", int.MinValue)]
    public void IntegerNode_WithIntegerPrecision_StoresIntValue(string literal, int expectedValue)
    {
      var node = new IntegerNode();
      var rpn = CreateStack();
      var element = new Element(new Token(literal, TokenType.Integer), ElementType.Integer);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);

      Assert.Equal(ExtendedType.Integer, node.ValueType);
      Assert.Equal(expectedValue, node.IntegerValue);
    }

    [Theory]
    [InlineData("2147483648")] // int.MaxValue + 1
    [InlineData("9223372036854775807")] // long.MaxValue
    [InlineData("-2147483649")] // int.MinValue - 1
    public void IntegerNode_WithIntegerPrecision_ThrowsOnOverflow(string literal)
    {
      var node = new IntegerNode();
      var rpn = CreateStack();
      var element = new Element(new Token(literal, TokenType.Integer), ElementType.Integer);

      Assert.Throws<System.OverflowException>(() =>
        node.Build(rpn, element,
          new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
          Compiler.Options.None, null));
    }

    [Fact]
    public void IntegerNode_Constructor_WithIntValue_CreatesIntegerType()
    {
      var node = new IntegerNode(42);
      Assert.Equal(ExtendedType.Integer, node.ValueType);
      Assert.Equal(42, node.IntegerValue);
    }

    [Fact]
    public void IntegerNode_Constructor_WithLongValue_CreatesLongType()
    {
      var node = new IntegerNode(3000000000L);
      Assert.Equal(ExtendedType.Long, node.ValueType);
      Assert.Equal(3000000000L, node.LongValue);
    }

    #endregion

    #region Float Precision Tests

    [Theory]
    [InlineData("0.0", 0.0f)]
    [InlineData("1.5", 1.5f)]
    [InlineData("3.14159", 3.14159f)]
    [InlineData("1e10", 1e10f)]
    public void FloatNode_WithFloatPrecision_StoresFloatValue(string literal, float expectedValue)
    {
      var node = new FloatNode();
      var rpn = CreateStack();
      var element = new Element(new Token(literal, TokenType.Float), ElementType.Float);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);

      Assert.Equal(ExtendedType.Float, node.ValueType);
      Assert.True(EpsilonScript.Math.IsNearlyEqual(expectedValue, node.FloatValue));
    }

    [Theory]
    [InlineData("0.0", 0.0)]
    [InlineData("1.5", 1.5)]
    [InlineData("3.141592653589793", 3.141592653589793)] // More precision than float
    [InlineData("1e100", 1e100)]
    [InlineData("1.7976931348623157e308", 1.7976931348623157e308)] // Near double.MaxValue
    public void FloatNode_WithDoublePrecision_StoresDoubleValue(string literal, double expectedValue)
    {
      var node = new FloatNode();
      var rpn = CreateStack();
      var element = new Element(new Token(literal, TokenType.Float), ElementType.Float);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double, null),
        Compiler.Options.None, null);

      Assert.Equal(ExtendedType.Double, node.ValueType);
      Assert.Equal(expectedValue, node.DoubleValue);
    }

    [Theory]
    [MemberData(nameof(DecimalTestData))]
    public void FloatNode_WithDecimalPrecision_StoresDecimalValue(string literal, decimal expectedValue)
    {
      var node = new FloatNode();
      var rpn = CreateStack();
      var element = new Element(new Token(literal, TokenType.Float), ElementType.Float);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal, null),
        Compiler.Options.None, null);

      Assert.Equal(ExtendedType.Decimal, node.ValueType);
      Assert.Equal(expectedValue, node.DecimalValue);
    }

    public static IEnumerable<object[]> DecimalTestData
    {
      get
      {
        return new[]
        {
          new object[] { "0.0", 0.0m },
          new object[] { "1.5", 1.5m },
          new object[] { "3.141592653589793238", 3.141592653589793238m }, // High precision
          new object[] { "0.0000000000000000001", 0.0000000000000000001m }, // Very small
          new object[] { "79228162514264337593543950335", 79228162514264337593543950335m }, // Near decimal.MaxValue
        };
      }
    }

    [Fact]
    public void FloatNode_Constructor_WithFloatValue_CreatesFloatType()
    {
      var node = new FloatNode(3.14f);
      Assert.Equal(ExtendedType.Float, node.ValueType);
      Assert.True(EpsilonScript.Math.IsNearlyEqual(3.14f, node.FloatValue));
    }

    [Fact]
    public void FloatNode_Constructor_WithDoubleValue_CreatesDoubleType()
    {
      var node = new FloatNode(3.141592653589793);
      Assert.Equal(ExtendedType.Double, node.ValueType);
      Assert.Equal(3.141592653589793, node.DoubleValue);
    }

    [Fact]
    public void FloatNode_Constructor_WithDecimalValue_CreatesDecimalType()
    {
      var node = new FloatNode(3.141592653589793238m);
      Assert.Equal(ExtendedType.Decimal, node.ValueType);
      Assert.Equal(3.141592653589793238m, node.DecimalValue);
    }

    #endregion

    #region Precision Comparison Tests

    [Fact]
    public void FloatPrecision_DoubleMaintainsMorePrecisionThanFloat()
    {
      // Value that loses precision in float but not in double
      var literal = "0.1234567890123456";

      var floatNode = new FloatNode();
      var floatElement = new Element(new Token(literal, TokenType.Float), ElementType.Float);
      floatNode.Build(CreateStack(), floatElement,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);

      var doubleNode = new FloatNode();
      var doubleElement = new Element(new Token(literal, TokenType.Float), ElementType.Float);
      doubleNode.Build(CreateStack(), doubleElement,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double, null),
        Compiler.Options.None, null);

      // Double should maintain more precision
      Assert.Equal(0.1234567890123456, doubleNode.DoubleValue, precision: 15);
      // Float loses precision
      Assert.NotEqual(0.1234567890123456, (double)floatNode.FloatValue, precision: 15);
    }

    [Fact]
    public void FloatPrecision_DecimalMaintainsMorePrecisionThanDouble()
    {
      // Value with very high precision
      var literal = "0.1234567890123456789012345678";

      var decimalNode = new FloatNode();
      var decimalElement = new Element(new Token(literal, TokenType.Float), ElementType.Float);
      decimalNode.Build(CreateStack(), decimalElement,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal, null),
        Compiler.Options.None, null);

      var doubleNode = new FloatNode();
      var doubleElement = new Element(new Token(literal, TokenType.Float), ElementType.Float);
      doubleNode.Build(CreateStack(), doubleElement,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double, null),
        Compiler.Options.None, null);

      // Decimal should maintain exact value
      Assert.Equal(0.1234567890123456789012345678m, decimalNode.DecimalValue);
      // Double loses precision
      Assert.NotEqual((double)0.1234567890123456789012345678m, doubleNode.DoubleValue);
    }

    #endregion

    #region Boundary Value Tests

    [Theory]
    [InlineData("9223372036854775806")] // long.MaxValue - 1
    [InlineData("9223372036854775807")] // long.MaxValue
    public void IntegerNode_WithLongPrecision_HandlesMaxValues(string literal)
    {
      var node = new IntegerNode();
      var rpn = CreateStack();
      var element = new Element(new Token(literal, TokenType.Integer), ElementType.Integer);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float, null), Compiler.Options.None,
        null);

      Assert.Equal(ExtendedType.Long, node.ValueType);
      Assert.Equal(long.Parse(literal), node.LongValue);
    }

    [Theory]
    [InlineData("-9223372036854775807")] // long.MinValue + 1
    [InlineData("-9223372036854775808")] // long.MinValue
    public void IntegerNode_WithLongPrecision_HandlesMinValues(string literal)
    {
      var node = new IntegerNode();
      var rpn = CreateStack();
      var element = new Element(new Token(literal, TokenType.Integer), ElementType.Integer);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float, null), Compiler.Options.None,
        null);

      Assert.Equal(ExtendedType.Long, node.ValueType);
      Assert.Equal(long.Parse(literal), node.LongValue);
    }

    [Theory]
    [InlineData("1.7976931348623157e308")] // Near double.MaxValue
    [InlineData("2.2250738585072014e-308")] // Near double.MinValue (positive)
    public void FloatNode_WithDoublePrecision_HandlesBoundaryValues(string literal)
    {
      var node = new FloatNode();
      var rpn = CreateStack();
      var element = new Element(new Token(literal, TokenType.Float), ElementType.Float);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double, null),
        Compiler.Options.None, null);

      Assert.Equal(ExtendedType.Double, node.ValueType);
      Assert.Equal(double.Parse(literal), node.DoubleValue);
    }

    #endregion
  }
}