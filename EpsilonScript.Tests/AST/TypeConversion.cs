using System;
using EpsilonScript.AST;
using EpsilonScript.Tests.TestInfrastructure;
using Xunit;

namespace EpsilonScript.Tests.AST
{
  [Trait("Category", "Unit")]
  [Trait("Component", "AST")]
  public class TypeConversion
  {
    #region GetValueAsInteger Tests

    [Theory]
    [InlineData(42, 42)]
    [InlineData(0, 0)]
    [InlineData(-100, -100)]
    [InlineData(int.MaxValue, int.MaxValue)]
    [InlineData(int.MinValue, int.MinValue)]
    public void GetValueAsInteger_FromInteger_ReturnsCorrectValue(int input, int expected)
    {
      var node = new IntegerNode(input);
      Assert.Equal(expected, node.IntegerValue);
    }

    [Theory]
    [InlineData(100L, 100)]
    [InlineData(0L, 0)]
    [InlineData(-100L, -100)]
    public void GetValueAsInteger_FromLong_ReturnsCorrectValue(long input, int expected)
    {
      var node = new IntegerNode(input);
      Assert.Equal(expected, node.IntegerValue);
    }

    [Theory]
    [InlineData(42.9f, 42)]
    [InlineData(0.5f, 0)]
    [InlineData(-10.7f, -10)]
    public void GetValueAsInteger_FromFloat_ReturnsTruncated(float input, int expected)
    {
      var node = new FloatNode(input);
      Assert.Equal(expected, node.IntegerValue);
    }

    [Theory]
    [InlineData(42.9, 42)]
    [InlineData(0.5, 0)]
    [InlineData(-10.7, -10)]
    public void GetValueAsInteger_FromDouble_ReturnsTruncated(double input, int expected)
    {
      var node = new FloatNode(input);
      Assert.Equal(expected, node.IntegerValue);
    }

    [Theory]
    [InlineData(42.9, 42)]
    [InlineData(0.5, 0)]
    [InlineData(-10.7, -10)]
    public void GetValueAsInteger_FromDecimal_ReturnsTruncated(double inputDouble, int expected)
    {
      var node = new FloatNode((decimal)inputDouble);
      Assert.Equal(expected, node.IntegerValue);
    }

    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 0)]
    public void GetValueAsInteger_FromBoolean_ReturnsZeroOrOne(bool input, int expected)
    {
      var node = new BooleanNode(input);
      Assert.Equal(expected, node.IntegerValue);
    }

    [Theory]
    [InlineData(float.PositiveInfinity, 0)]
    [InlineData(float.NegativeInfinity, 0)]
    [InlineData(float.NaN, 0)]
    public void GetValueAsInteger_FromFloatSpecialValues_ReturnsSafeValue(float input, int expected)
    {
      var node = new FloatNode(input);
      Assert.Equal(expected, node.IntegerValue);
    }

    #endregion

    #region GetValueAsLong Tests

    [Theory]
    [InlineData(42, 42L)]
    [InlineData(0, 0L)]
    [InlineData(-100, -100L)]
    public void GetValueAsLong_FromInteger_ReturnsCorrectValue(int input, long expected)
    {
      var node = new IntegerNode(input);
      Assert.Equal(expected, node.LongValue);
    }

    [Theory]
    [InlineData(5000000000L, 5000000000L)]
    [InlineData(0L, 0L)]
    [InlineData(-5000000000L, -5000000000L)]
    [InlineData(long.MaxValue, long.MaxValue)]
    [InlineData(long.MinValue, long.MinValue)]
    public void GetValueAsLong_FromLong_ReturnsCorrectValue(long input, long expected)
    {
      var node = new IntegerNode(input);
      Assert.Equal(expected, node.LongValue);
    }

    [Theory]
    [InlineData(42.9f, 42L)]
    [InlineData(0.5f, 0L)]
    [InlineData(-10.7f, -10L)]
    public void GetValueAsLong_FromFloat_ReturnsTruncated(float input, long expected)
    {
      var node = new FloatNode(input);
      Assert.Equal(expected, node.LongValue);
    }

    [Theory]
    [InlineData(42.9, 42L)]
    [InlineData(0.5, 0L)]
    [InlineData(-10.7, -10L)]
    public void GetValueAsLong_FromDouble_ReturnsTruncated(double input, long expected)
    {
      var node = new FloatNode(input);
      Assert.Equal(expected, node.LongValue);
    }

    [Theory]
    [InlineData(true, 1L)]
    [InlineData(false, 0L)]
    public void GetValueAsLong_FromBoolean_ReturnsZeroOrOne(bool input, long expected)
    {
      var node = new BooleanNode(input);
      Assert.Equal(expected, node.LongValue);
    }

    #endregion

    #region GetValueAsFloat Tests

    [Theory]
    [InlineData(42, 42.0f)]
    [InlineData(0, 0.0f)]
    [InlineData(-100, -100.0f)]
    public void GetValueAsFloat_FromInteger_ReturnsCorrectValue(int input, float expected)
    {
      var node = new IntegerNode(input);
      Assert.True(EpsilonScript.Math.IsNearlyEqual(expected, node.FloatValue));
    }

    [Theory]
    [InlineData(100L, 100.0f)]
    [InlineData(0L, 0.0f)]
    [InlineData(-100L, -100.0f)]
    public void GetValueAsFloat_FromLong_ReturnsCorrectValue(long input, float expected)
    {
      var node = new IntegerNode(input);
      Assert.True(EpsilonScript.Math.IsNearlyEqual(expected, node.FloatValue));
    }

    [Theory]
    [InlineData(3.14f, 3.14f)]
    [InlineData(0.0f, 0.0f)]
    [InlineData(-2.5f, -2.5f)]
    public void GetValueAsFloat_FromFloat_ReturnsCorrectValue(float input, float expected)
    {
      var node = new FloatNode(input);
      Assert.True(EpsilonScript.Math.IsNearlyEqual(expected, node.FloatValue));
    }

    [Theory]
    [InlineData(3.14, 3.14f)]
    [InlineData(0.0, 0.0f)]
    [InlineData(-2.5, -2.5f)]
    public void GetValueAsFloat_FromDouble_ReturnsCorrectValue(double input, float expected)
    {
      var node = new FloatNode(input);
      Assert.True(EpsilonScript.Math.IsNearlyEqual(expected, node.FloatValue));
    }

    [Theory]
    [InlineData(true, 1.0f)]
    [InlineData(false, 0.0f)]
    public void GetValueAsFloat_FromBoolean_ReturnsZeroOrOne(bool input, float expected)
    {
      var node = new BooleanNode(input);
      Assert.True(EpsilonScript.Math.IsNearlyEqual(expected, node.FloatValue));
    }

    #endregion

    #region GetValueAsDouble Tests

    [Theory]
    [InlineData(42, 42.0)]
    [InlineData(0, 0.0)]
    [InlineData(-100, -100.0)]
    public void GetValueAsDouble_FromInteger_ReturnsCorrectValue(int input, double expected)
    {
      var node = new IntegerNode(input);
      Assert.Equal(expected, node.DoubleValue);
    }

    [Theory]
    [InlineData(5000000000L, 5000000000.0)]
    [InlineData(0L, 0.0)]
    [InlineData(-5000000000L, -5000000000.0)]
    public void GetValueAsDouble_FromLong_ReturnsCorrectValue(long input, double expected)
    {
      var node = new IntegerNode(input);
      Assert.Equal(expected, node.DoubleValue);
    }

    [Theory]
    [InlineData(3.14f, 3.14)]
    [InlineData(0.0f, 0.0)]
    [InlineData(-2.5f, -2.5)]
    public void GetValueAsDouble_FromFloat_ReturnsCorrectValue(float input, double expected)
    {
      var node = new FloatNode(input);
      Assert.Equal(expected, node.DoubleValue, precision: 5);
    }

    [Theory]
    [InlineData(3.141592653589793, 3.141592653589793)]
    [InlineData(0.0, 0.0)]
    [InlineData(-2.5, -2.5)]
    public void GetValueAsDouble_FromDouble_ReturnsCorrectValue(double input, double expected)
    {
      var node = new FloatNode(input);
      Assert.Equal(expected, node.DoubleValue);
    }

    [Theory]
    [InlineData(true, 1.0)]
    [InlineData(false, 0.0)]
    public void GetValueAsDouble_FromBoolean_ReturnsZeroOrOne(bool input, double expected)
    {
      var node = new BooleanNode(input);
      Assert.Equal(expected, node.DoubleValue);
    }

    #endregion

    #region GetValueAsDecimal Tests

    [Theory]
    [InlineData(42)]
    [InlineData(0)]
    [InlineData(-100)]
    public void GetValueAsDecimal_FromInteger_ReturnsCorrectValue(int input)
    {
      var node = new IntegerNode(input);
      Assert.Equal((decimal)input, node.DecimalValue);
    }

    [Theory]
    [InlineData(5000000000L)]
    [InlineData(0L)]
    [InlineData(-5000000000L)]
    public void GetValueAsDecimal_FromLong_ReturnsCorrectValue(long input)
    {
      var node = new IntegerNode(input);
      Assert.Equal((decimal)input, node.DecimalValue);
    }

    [Fact]
    public void GetValueAsDecimal_FromFloat_ReturnsCorrectValue()
    {
      // Test float to decimal conversion
      // Note: FloatNode stores float as double internally, so conversion path is float→double→decimal
      var node1 = new FloatNode(3.14f);
      // Conversion: 3.14f → double → decimal
      var expected1 = (decimal)(double)3.14f;
      Assert.Equal(expected1, node1.DecimalValue);

      var node2 = new FloatNode(0.0f);
      Assert.Equal(0m, node2.DecimalValue);

      var node3 = new FloatNode(-2.5f);
      var expected3 = (decimal)(double)(-2.5f);
      Assert.Equal(expected3, node3.DecimalValue);
    }

    [Theory]
    [InlineData(3.141592653589793)]
    [InlineData(0.0)]
    [InlineData(-2.5)]
    public void GetValueAsDecimal_FromDouble_ReturnsCorrectValue(double input)
    {
      var node = new FloatNode(input);
      Assert.Equal((decimal)input, node.DecimalValue);
    }

    [Fact]
    public void GetValueAsDecimal_FromDecimal_ReturnsExactValue()
    {
      var input = 1.23456789012345678901234567m;
      var node = new FloatNode(input);
      Assert.Equal(input, node.DecimalValue);
    }

    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 0)]
    public void GetValueAsDecimal_FromBoolean_ReturnsZeroOrOne(bool input, int expectedInt)
    {
      var node = new BooleanNode(input);
      Assert.Equal((decimal)expectedInt, node.DecimalValue);
    }

    #endregion

    #region Cross-Type Conversion Tests

    [Fact]
    public void TypeConversion_IntegerToAllTypes_WorksCorrectly()
    {
      var node = new IntegerNode(42);

      Assert.Equal(42, node.IntegerValue);
      Assert.Equal(42L, node.LongValue);
      Assert.True(EpsilonScript.Math.IsNearlyEqual(42.0f, node.FloatValue));
      Assert.Equal(42.0, node.DoubleValue);
      Assert.Equal(42m, node.DecimalValue);
    }

    [Fact]
    public void TypeConversion_LongToAllTypes_WorksCorrectly()
    {
      var node = new IntegerNode(5000000000L);

      Assert.Equal(5000000000L, node.LongValue);
      Assert.Equal(5000000000.0f, node.FloatValue);
      Assert.Equal(5000000000.0, node.DoubleValue);
      Assert.Equal(5000000000m, node.DecimalValue);
    }

    [Fact]
    public void TypeConversion_FloatToAllTypes_WorksCorrectly()
    {
      var node = new FloatNode(3.14f);

      Assert.Equal(3, node.IntegerValue); // Truncated
      Assert.Equal(3L, node.LongValue); // Truncated
      Assert.True(EpsilonScript.Math.IsNearlyEqual(3.14f, node.FloatValue));
      Assert.Equal(3.14, node.DoubleValue, precision: 5);
      Assert.Equal(3.14m, node.DecimalValue, precision: 5);
    }

    [Fact]
    public void TypeConversion_DoubleToAllTypes_WorksCorrectly()
    {
      var node = new FloatNode(3.141592653589793);

      Assert.Equal(3, node.IntegerValue); // Truncated
      Assert.Equal(3L, node.LongValue); // Truncated
      Assert.Equal((float)3.141592653589793, node.FloatValue);
      Assert.Equal(3.141592653589793, node.DoubleValue);
      Assert.Equal((decimal)3.141592653589793, node.DecimalValue);
    }

    [Fact]
    public void TypeConversion_DecimalToAllTypes_WorksCorrectly()
    {
      var node = new FloatNode(3.141592653589793238m);

      Assert.Equal(3, node.IntegerValue); // Truncated
      Assert.Equal(3L, node.LongValue); // Truncated
      Assert.Equal((float)3.141592653589793238m, node.FloatValue);
      Assert.Equal((double)3.141592653589793238m, node.DoubleValue);
      Assert.Equal(3.141592653589793238m, node.DecimalValue);
    }

    [Fact]
    public void TypeConversion_BooleanToAllTypes_WorksCorrectly()
    {
      var trueNode = new BooleanNode(true);
      Assert.Equal(1, trueNode.IntegerValue);
      Assert.Equal(1L, trueNode.LongValue);
      Assert.True(EpsilonScript.Math.IsNearlyEqual(1.0f, trueNode.FloatValue));
      Assert.Equal(1.0, trueNode.DoubleValue);
      Assert.Equal(1m, trueNode.DecimalValue);

      var falseNode = new BooleanNode(false);
      Assert.Equal(0, falseNode.IntegerValue);
      Assert.Equal(0L, falseNode.LongValue);
      Assert.True(EpsilonScript.Math.IsNearlyEqual(0.0f, falseNode.FloatValue));
      Assert.Equal(0.0, falseNode.DoubleValue);
      Assert.Equal(0m, falseNode.DecimalValue);
    }

    #endregion

    #region Overflow and Edge Case Tests

    [Fact]
    public void TypeConversion_LongToInteger_LargeValue_Truncates()
    {
      // When a long value is too large for int, it truncates (wraps)
      var node = new IntegerNode(long.MaxValue);
      // Truncation behavior: long.MaxValue cast to int wraps around
      unchecked
      {
        Assert.Equal((int)long.MaxValue, node.IntegerValue);
      }
    }

    [Fact]
    public void TypeConversion_LongToInteger_SmallValue_Truncates()
    {
      // When a long value is too small for int, it truncates (wraps)
      var node = new IntegerNode(long.MinValue);
      // Truncation behavior: long.MinValue cast to int wraps around
      unchecked
      {
        Assert.Equal((int)long.MinValue, node.IntegerValue);
      }
    }

    [Fact]
    public void TypeConversion_FloatToInteger_Overflow_UncheckedCast()
    {
      var node = new FloatNode(float.MaxValue);
      // Unchecked runtime cast behavior - float stored as double, then cast to int
      // This matches: float f = float.MaxValue; double d = f; int i = (int)d;
      var f = float.MaxValue;
      double d = f;
      Assert.Equal((int)d, node.IntegerValue);
    }

    [Fact]
    public void TypeConversion_FloatToInteger_Underflow_UncheckedCast()
    {
      var node = new FloatNode(-float.MaxValue);
      // Unchecked runtime cast behavior - float stored as double, then cast to int
      var f = -float.MaxValue;
      double d = f;
      Assert.Equal((int)d, node.IntegerValue);
    }

    [Fact]
    public void TypeConversion_DoubleToInteger_Overflow_UncheckedCast()
    {
      var node = new FloatNode(double.MaxValue);
      // Unchecked runtime cast behavior
      var d = double.MaxValue;
      Assert.Equal((int)d, node.IntegerValue);
    }

    [Fact]
    public void TypeConversion_DoubleToInteger_Underflow_UncheckedCast()
    {
      var node = new FloatNode(-double.MaxValue);
      // Unchecked runtime cast behavior
      var d = -double.MaxValue;
      Assert.Equal((int)d, node.IntegerValue);
    }

    [Fact]
    public void TypeConversion_FloatToLong_LargeValue_PreservesValue()
    {
      var node = new FloatNode(3_000_000_000.0f);

      Assert.Equal(3_000_000_000L, node.LongValue);
    }

    [Fact]
    public void TypeConversion_DoubleToLong_LargeValue_PreservesValue()
    {
      var node = new FloatNode(5_000_000_000.0);

      Assert.Equal(5_000_000_000L, node.LongValue);
    }

    [Fact]
    public void TypeConversion_FloatToLong_NegativeLargeValue_PreservesValue()
    {
      var node = new FloatNode(-3_000_000_000.0f);

      Assert.Equal(-3_000_000_000L, node.LongValue);
    }

    [Fact]
    public void TypeConversion_DoubleToLong_NegativeLargeValue_PreservesValue()
    {
      var node = new FloatNode(-5_000_000_000.0);

      Assert.Equal(-5_000_000_000L, node.LongValue);
    }

    [Fact]
    public void TypeConversion_DecimalToInteger_Overflow_ThrowsException()
    {
      var node = new FloatNode((decimal)int.MaxValue + 1000m);

      Assert.Throws<OverflowException>(() => node.IntegerValue);
    }

    [Fact]
    public void TypeConversion_DecimalToInteger_Underflow_ThrowsException()
    {
      var node = new FloatNode((decimal)int.MinValue - 1000m);

      Assert.Throws<OverflowException>(() => node.IntegerValue);
    }

    [Fact]
    public void TypeConversion_DecimalToInteger_VeryLargeValue_ThrowsException()
    {
      var node = new FloatNode(decimal.MaxValue);

      Assert.Throws<OverflowException>(() => node.IntegerValue);
    }

    #endregion

    #region Precision Loss Tests

    [Fact]
    public void TypeConversion_DoubleToFloat_LosesPrecision()
    {
      var doubleValue = 0.123456789012345;
      var node = new FloatNode(doubleValue);

      var asFloat = node.FloatValue;
      var asDouble = node.DoubleValue;

      // Float has less precision than double
      Assert.NotEqual(doubleValue, (double)asFloat, precision: 15);
      Assert.Equal(doubleValue, asDouble, precision: 15);
    }

    [Fact]
    public void TypeConversion_DecimalToDouble_MaintainsReasonablePrecision()
    {
      var decimalValue = 0.1234567890123456m;
      var node = new FloatNode(decimalValue);

      var asDouble = node.DoubleValue;
      var asDecimal = node.DecimalValue;

      // Double maintains decent precision but not as exact as decimal
      Assert.Equal((double)decimalValue, asDouble, precision: 15);
      Assert.Equal(decimalValue, asDecimal);
    }

    #endregion
  }
}