using System;
using Xunit;

namespace EpsilonScript.Tests.VariableValueTests
{
  [Trait("Category", "Unit")]
  [Trait("Component", "VariableValue")]
  public class VariableValueTypeConversionTests
  {
    #region Float/Double to Long Conversion Tests

    [Fact]
    public void FloatToLong_LargeValue_PreservesValue()
    {
      var variable = new VariableValue(3_000_000_000.0f);

      Assert.Equal(3_000_000_000L, variable.LongValue);
    }

    [Fact]
    public void DoubleToLong_LargeValue_PreservesValue()
    {
      var variable = new VariableValue(5_000_000_000.0);

      Assert.Equal(5_000_000_000L, variable.LongValue);
    }

    [Fact]
    public void FloatToLong_NegativeLargeValue_PreservesValue()
    {
      var variable = new VariableValue(-3_000_000_000.0f);

      Assert.Equal(-3_000_000_000L, variable.LongValue);
    }

    [Fact]
    public void DoubleToLong_NegativeLargeValue_PreservesValue()
    {
      var variable = new VariableValue(-5_000_000_000.0);

      Assert.Equal(-5_000_000_000L, variable.LongValue);
    }

    [Fact]
    public void FloatToLong_WithinIntRange_WorksCorrectly()
    {
      var variable = new VariableValue(1_000_000_000.0f);

      Assert.Equal(1_000_000_000L, variable.LongValue);
      Assert.Equal(1_000_000_000, variable.IntegerValue);
    }

    [Fact]
    public void DoubleToLong_BoundaryAtIntMax_PreservesValue()
    {
      var variable = new VariableValue((double)int.MaxValue + 1.0);

      Assert.Equal((long)int.MaxValue + 1L, variable.LongValue);
    }

    [Fact]
    public void DoubleToLong_BoundaryAtIntMin_PreservesValue()
    {
      var variable = new VariableValue((double)int.MinValue - 1.0);

      Assert.Equal((long)int.MinValue - 1L, variable.LongValue);
    }

    #endregion

    #region Decimal to Integer Conversion Tests

    [Fact]
    public void DecimalToInteger_Overflow_ThrowsException()
    {
      var variable = new VariableValue((decimal)int.MaxValue + 1000m);

      Assert.Throws<OverflowException>(() => variable.IntegerValue);
    }

    [Fact]
    public void DecimalToInteger_Underflow_ThrowsException()
    {
      var variable = new VariableValue((decimal)int.MinValue - 1000m);

      Assert.Throws<OverflowException>(() => variable.IntegerValue);
    }

    [Fact]
    public void DecimalToInteger_VeryLargeValue_ThrowsException()
    {
      var variable = new VariableValue(decimal.MaxValue);

      Assert.Throws<OverflowException>(() => variable.IntegerValue);
    }

    [Fact]
    public void DecimalToInteger_NormalValue_WorksCorrectly()
    {
      var variable = new VariableValue(42.5m);

      Assert.Equal(42, variable.IntegerValue);
    }

    #endregion

    #region Safe Type Access Tests

    [Fact]
    public void SafeTypeAccess_IntegerToLong_WorksCorrectly()
    {
      var variable = new VariableValue(42);

      Assert.Equal(Type.Integer, variable.Type);
      Assert.Equal(42L, variable.LongValue);
    }

    [Fact]
    public void SafeTypeAccess_FloatToDouble_WorksCorrectly()
    {
      var variable = new VariableValue(3.14f);

      Assert.Equal(Type.Float, variable.Type);
      Assert.Equal(3.14, variable.DoubleValue, precision: 5);
    }

    [Fact]
    public void SafeTypeAccess_DecimalToDouble_WorksCorrectly()
    {
      var variable = new VariableValue(3.141592653589793238m);

      Assert.Equal(Type.Decimal, variable.Type);
      Assert.Equal((double)3.141592653589793238m, variable.DoubleValue);
    }

    [Fact]
    public void SafeTypeAccess_LongToInteger_WorksCorrectly()
    {
      var variable = new VariableValue(42L);

      Assert.Equal(Type.Long, variable.Type);
      Assert.Equal(42, variable.IntegerValue);
    }

    [Fact]
    public void BooleanToNumeric_ConvertsToOneOrZero()
    {
      var variableTrue = new VariableValue(true);
      var variableFalse = new VariableValue(false);

      // True converts to 1
      Assert.Equal(1.0f, variableTrue.FloatValue);
      Assert.Equal(1.0, variableTrue.DoubleValue);
      Assert.Equal(1m, variableTrue.DecimalValue);

      // False converts to 0
      Assert.Equal(0.0f, variableFalse.FloatValue);
      Assert.Equal(0.0, variableFalse.DoubleValue);
      Assert.Equal(0m, variableFalse.DecimalValue);
    }

    #endregion

    #region Cross-Caching Verification Tests

    [Fact]
    public void IntegerValue_PopulatesCrossCache()
    {
      var variable = new VariableValue(0);
      variable.IntegerValue = 42;

      Assert.Equal(Type.Integer, variable.Type);
      Assert.Equal(42, variable.IntegerValue);
      Assert.Equal(42L, variable.LongValue);
      Assert.Equal(42.0f, variable.FloatValue);
      Assert.Equal(42.0, variable.DoubleValue);
      Assert.True(variable.BooleanValue);
    }

    [Fact]
    public void DecimalValue_DoesNotPopulateCrossCache()
    {
      var variable = new VariableValue(0);
      variable.DecimalValue = 42m;

      // Type should be preserved as Integer
      Assert.Equal(Type.Integer, variable.Type);
      // Value is stored as integer, converts when accessed as decimal
      Assert.Equal(42m, variable.DecimalValue);
      Assert.Equal(42, variable.IntegerValue);
      Assert.Equal(42L, variable.LongValue);
    }

    #endregion

    #region String to Numeric Parsing Tests

    [Fact]
    public void StringToInteger_ValidString_ParsesCorrectly()
    {
      var variable = new VariableValue("42");

      Assert.Equal(42, variable.IntegerValue);
    }

    [Fact]
    public void StringToInteger_NegativeString_ParsesCorrectly()
    {
      var variable = new VariableValue("-123");

      Assert.Equal(-123, variable.IntegerValue);
    }

    [Fact]
    public void StringToInteger_InvalidString_ThrowsException()
    {
      var variable = new VariableValue("not a number");

      Assert.Throws<InvalidCastException>(() => variable.IntegerValue);
    }

    [Fact]
    public void StringToLong_ValidString_ParsesCorrectly()
    {
      var variable = new VariableValue("3000000000");

      Assert.Equal(3000000000L, variable.LongValue);
    }

    [Fact]
    public void StringToLong_NegativeString_ParsesCorrectly()
    {
      var variable = new VariableValue("-9223372036854775808");

      Assert.Equal(long.MinValue, variable.LongValue);
    }

    [Fact]
    public void StringToLong_InvalidString_ThrowsException()
    {
      var variable = new VariableValue("not a number");

      Assert.Throws<InvalidCastException>(() => variable.LongValue);
    }

    [Fact]
    public void StringToFloat_ValidString_ParsesCorrectly()
    {
      var variable = new VariableValue("3.14");

      Assert.Equal(3.14f, variable.FloatValue, precision: 5);
    }

    [Fact]
    public void StringToFloat_NegativeString_ParsesCorrectly()
    {
      var variable = new VariableValue("-2.5");

      Assert.Equal(-2.5f, variable.FloatValue);
    }

    [Fact]
    public void StringToFloat_ScientificNotation_ParsesCorrectly()
    {
      var variable = new VariableValue("1.5e10");

      Assert.Equal(1.5e10f, variable.FloatValue);
    }

    [Fact]
    public void StringToFloat_InvalidString_ThrowsException()
    {
      var variable = new VariableValue("not a number");

      Assert.Throws<InvalidCastException>(() => variable.FloatValue);
    }

    [Fact]
    public void StringToDouble_ValidString_ParsesCorrectly()
    {
      var variable = new VariableValue("3.141592653589793");

      Assert.Equal(3.141592653589793, variable.DoubleValue);
    }

    [Fact]
    public void StringToDouble_NegativeString_ParsesCorrectly()
    {
      var variable = new VariableValue("-2.718281828459045");

      Assert.Equal(-2.718281828459045, variable.DoubleValue);
    }

    [Fact]
    public void StringToDouble_ScientificNotation_ParsesCorrectly()
    {
      var variable = new VariableValue("6.022e23");

      Assert.Equal(6.022e23, variable.DoubleValue);
    }

    [Fact]
    public void StringToDouble_InvalidString_ThrowsException()
    {
      var variable = new VariableValue("not a number");

      Assert.Throws<InvalidCastException>(() => variable.DoubleValue);
    }

    [Fact]
    public void StringToDecimal_ValidString_ParsesCorrectly()
    {
      var variable = new VariableValue("123.456789");

      Assert.Equal(123.456789m, variable.DecimalValue);
    }

    [Fact]
    public void StringToDecimal_NegativeString_ParsesCorrectly()
    {
      var variable = new VariableValue("-987.654321");

      Assert.Equal(-987.654321m, variable.DecimalValue);
    }

    [Fact]
    public void StringToDecimal_LargeValue_ParsesCorrectly()
    {
      var variable = new VariableValue("79228162514264337593543950335");

      Assert.Equal(decimal.MaxValue, variable.DecimalValue);
    }

    [Fact]
    public void StringToDecimal_InvalidString_ThrowsException()
    {
      var variable = new VariableValue("not a number");

      Assert.Throws<InvalidCastException>(() => variable.DecimalValue);
    }

    #endregion
  }
}