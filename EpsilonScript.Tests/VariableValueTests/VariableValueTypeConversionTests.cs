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

    #region On-Demand Type Conversion Tests

    [Fact]
    public void IntegerValue_ConvertsToAllNumericTypes_Correctly()
    {
      // Verifies that when a VariableValue stores an integer,
      // it can be read as long, float, double, or boolean
      // through on-demand type conversion (no caching involved)
      var variable = new VariableValue(0);
      variable.IntegerValue = 42;

      Assert.Equal(Type.Integer, variable.Type);
      Assert.Equal(42, variable.IntegerValue); // Direct read
      Assert.Equal(42L, variable.LongValue); // On-demand conversion to long
      Assert.Equal(42.0f, variable.FloatValue); // On-demand conversion to float
      Assert.Equal(42.0, variable.DoubleValue); // On-demand conversion to double
      Assert.True(variable.BooleanValue); // On-demand conversion to bool (42 != 0)
    }

    [Fact]
    public void DecimalValue_AssignedToIntegerVariable_ConvertsAndPreservesIntegerType()
    {
      // Verifies that assigning a decimal value to an integer-typed variable
      // converts the value to integer and preserves the Integer type.
      // Reading as decimal performs on-demand conversion back.
      var variable = new VariableValue(0);
      variable.DecimalValue = 42m; // Converts: _value.IntValue = (long)42m

      // Type should be preserved as Integer
      Assert.Equal(Type.Integer, variable.Type);
      // Value is stored as integer, converts when accessed as decimal
      Assert.Equal(42m, variable.DecimalValue); // On-demand: (decimal)_value.IntValue
      Assert.Equal(42, variable.IntegerValue); // Direct: (int)_value.IntValue
      Assert.Equal(42L, variable.LongValue); // Direct: _value.IntValue
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

    #region Numeric to String Conversion Tests

    [Fact]
    public void IntegerToString_ConvertsCorrectly()
    {
      var variable = new VariableValue(42);

      Assert.Equal("42", variable.StringValue);
    }

    [Fact]
    public void IntegerToString_NegativeValue_ConvertsCorrectly()
    {
      var variable = new VariableValue(-123);

      Assert.Equal("-123", variable.StringValue);
    }

    [Fact]
    public void LongToString_ConvertsCorrectly()
    {
      var variable = new VariableValue(3000000000L);

      Assert.Equal("3000000000", variable.StringValue);
    }

    [Fact]
    public void LongToString_NegativeValue_ConvertsCorrectly()
    {
      var variable = new VariableValue(-9223372036854775808L);

      Assert.Equal("-9223372036854775808", variable.StringValue);
    }

    [Fact]
    public void FloatToString_ConvertsCorrectly()
    {
      var variable = new VariableValue(3.14f);

      Assert.Equal("3.14", variable.StringValue);
    }

    [Fact]
    public void DoubleToString_ConvertsCorrectly()
    {
      var variable = new VariableValue(3.141592653589793);

      Assert.Equal("3.141592653589793", variable.StringValue);
    }

    [Fact]
    public void DecimalToString_ConvertsCorrectly()
    {
      var variable = new VariableValue(123.456789m);

      Assert.Equal("123.456789", variable.StringValue);
    }

    [Fact]
    public void BooleanToString_TrueValue_ConvertsCorrectly()
    {
      var variable = new VariableValue(true);

      Assert.Equal("True", variable.StringValue);
    }

    [Fact]
    public void BooleanToString_FalseValue_ConvertsCorrectly()
    {
      var variable = new VariableValue(false);

      Assert.Equal("False", variable.StringValue);
    }

    [Fact]
    public void StringValue_OnStringType_ReturnsOriginalValue()
    {
      var variable = new VariableValue("hello world");

      Assert.Equal("hello world", variable.StringValue);
    }

    #endregion

    #region Setter on String-Typed Variable Tests

    [Fact]
    public void StringVariable_IntegerSetter_ConvertsToString()
    {
      var variable = new VariableValue("original");
      variable.IntegerValue = 42;

      Assert.Equal(Type.String, variable.Type);
      Assert.Equal("42", variable.StringValue);
      Assert.Equal(42, variable.IntegerValue);
    }

    [Fact]
    public void StringVariable_NegativeIntegerSetter_ConvertsToString()
    {
      var variable = new VariableValue("original");
      variable.IntegerValue = -123;

      Assert.Equal(Type.String, variable.Type);
      Assert.Equal("-123", variable.StringValue);
      Assert.Equal(-123, variable.IntegerValue);
    }

    [Fact]
    public void StringVariable_LongSetter_ConvertsToString()
    {
      var variable = new VariableValue("original");
      variable.LongValue = 3000000000L;

      Assert.Equal(Type.String, variable.Type);
      Assert.Equal("3000000000", variable.StringValue);
      Assert.Equal(3000000000L, variable.LongValue);
    }

    [Fact]
    public void StringVariable_FloatSetter_ConvertsToString()
    {
      var variable = new VariableValue("original");
      variable.FloatValue = 3.14f;

      Assert.Equal(Type.String, variable.Type);
      Assert.Equal("3.14", variable.StringValue);
      Assert.Equal(3.14f, variable.FloatValue, precision: 5);
    }

    [Fact]
    public void StringVariable_DoubleSetter_ConvertsToString()
    {
      var variable = new VariableValue("original");
      variable.DoubleValue = 3.141592653589793;

      Assert.Equal(Type.String, variable.Type);
      Assert.Equal("3.141592653589793", variable.StringValue);
      Assert.Equal(3.141592653589793, variable.DoubleValue);
    }

    [Fact]
    public void StringVariable_DecimalSetter_ConvertsToString()
    {
      var variable = new VariableValue("original");
      variable.DecimalValue = 123.456789m;

      Assert.Equal(Type.String, variable.Type);
      Assert.Equal("123.456789", variable.StringValue);
      Assert.Equal(123.456789m, variable.DecimalValue);
    }

    [Fact]
    public void StringVariable_RoundTrip_Integer_PreservesValue()
    {
      var variable = new VariableValue("test");
      variable.IntegerValue = 42;
      int result = variable.IntegerValue;

      Assert.Equal(42, result);
    }

    [Fact]
    public void StringVariable_RoundTrip_Long_PreservesValue()
    {
      var variable = new VariableValue("test");
      variable.LongValue = 9223372036854775807L;
      long result = variable.LongValue;

      Assert.Equal(9223372036854775807L, result);
    }

    [Fact]
    public void StringVariable_RoundTrip_Float_PreservesValue()
    {
      var variable = new VariableValue("test");
      variable.FloatValue = 3.14f;
      float result = variable.FloatValue;

      Assert.Equal(3.14f, result, precision: 5);
    }

    [Fact]
    public void StringVariable_RoundTrip_Double_PreservesValue()
    {
      var variable = new VariableValue("test");
      variable.DoubleValue = 2.718281828459045;
      double result = variable.DoubleValue;

      Assert.Equal(2.718281828459045, result);
    }

    [Fact]
    public void StringVariable_RoundTrip_Decimal_PreservesValue()
    {
      var variable = new VariableValue("test");
      variable.DecimalValue = 123.456789012345678901234567890m;
      decimal result = variable.DecimalValue;

      Assert.Equal(123.456789012345678901234567890m, result);
    }

    #endregion
  }
}