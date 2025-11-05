using Xunit;

namespace EpsilonScript.Tests.CompiledScriptTests
{
  [Trait("Category", "Unit")]
  [Trait("Component", "CompiledScript")]
  public class CompiledScriptPrecisionTests
  {
    #region Integer Precision Tests

    [Fact]
    public void WithIntegerPrecision_CompilesIntegerValues()
    {
      var compiler = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      var script = compiler.Compile("42");

      script.Execute();

      Assert.Equal(Compiler.IntegerPrecision.Integer, script.IntegerPrecision);
      Assert.Equal(Type.Integer, script.ValueType);
      Assert.Equal(42, script.IntegerValue);
    }

    [Fact]
    public void WithLongPrecision_CompilesLongValues()
    {
      var compiler = new Compiler(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float);
      var script = compiler.Compile("3000000000"); // Beyond int32 range

      script.Execute();

      Assert.Equal(Compiler.IntegerPrecision.Long, script.IntegerPrecision);
      Assert.Equal(Type.Long, script.ValueType);
      Assert.Equal(3000000000L, script.LongValue);
    }

    [Fact]
    public void WithIntegerPrecision_ThrowsOnLongLiteral()
    {
      var compiler = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);

      Assert.Throws<System.OverflowException>(() => compiler.Compile("3000000000"));
    }

    [Theory]
    [InlineData("2147483648", 2147483648L)] // int.MaxValue + 1
    [InlineData("9223372036854775807", 9223372036854775807L)] // long.MaxValue
    [InlineData("-2147483649", -2147483649L)]
    // long.MinValue + 1 (MinValue itself can't be parsed due to negation overflow)
    [InlineData("-9223372036854775807", -9223372036854775807L)]
    public void WithLongPrecision_AcceptsLargeIntegerLiterals(string literal, long expectedValue)
    {
      var compiler = new Compiler(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float);
      var script = compiler.Compile(literal);

      script.Execute();

      Assert.Equal(Compiler.IntegerPrecision.Long, script.IntegerPrecision);
      Assert.Equal(Type.Long, script.ValueType);
      Assert.Equal(expectedValue, script.LongValue);
    }

    [Fact]
    public void WithLongPrecision_IntegerArithmeticProducesLong()
    {
      var compiler = new Compiler(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float);
      var script = compiler.Compile("100 + 200");

      script.Execute();

      Assert.Equal(Type.Long, script.ValueType);
      Assert.Equal(300L, script.LongValue);
    }

    #endregion

    #region Float Precision Tests

    [Fact]
    public void WithFloatPrecision_CompilesFloatValues()
    {
      var compiler = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      var script = compiler.Compile("3.14");

      script.Execute();

      Assert.Equal(Compiler.FloatPrecision.Float, script.FloatPrecision);
      Assert.Equal(Type.Float, script.ValueType);
      Assert.True(EpsilonScript.Math.IsNearlyEqual(3.14f, script.FloatValue));
    }

    [Fact]
    public void WithDoublePrecision_CompilesDoubleValues()
    {
      var compiler = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double);
      var script = compiler.Compile("3.141592653589793");

      script.Execute();

      Assert.Equal(Compiler.FloatPrecision.Double, script.FloatPrecision);
      Assert.Equal(Type.Double, script.ValueType);
      Assert.Equal(3.141592653589793, script.DoubleValue);
    }

    [Fact]
    public void WithDecimalPrecision_CompilesDecimalValues()
    {
      var compiler = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal);
      var script = compiler.Compile("3.141592653589793238");

      script.Execute();

      Assert.Equal(Compiler.FloatPrecision.Decimal, script.FloatPrecision);
      Assert.Equal(Type.Decimal, script.ValueType);
      Assert.Equal(3.141592653589793238m, script.DecimalValue);
    }

    [Fact]
    public void WithDoublePrecision_FloatArithmeticProducesDouble()
    {
      var compiler = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double);
      var script = compiler.Compile("1.5 + 2.5");

      script.Execute();

      Assert.Equal(Type.Double, script.ValueType);
      Assert.Equal(4.0, script.DoubleValue);
    }

    [Fact]
    public void WithDecimalPrecision_FloatArithmeticProducesDecimal()
    {
      var compiler = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal);
      var script = compiler.Compile("0.1 + 0.2");

      script.Execute();

      Assert.Equal(Type.Decimal, script.ValueType);
      Assert.Equal(0.3m, script.DecimalValue); // Exact decimal arithmetic
    }

    #endregion

    #region Combined Precision Tests

    [Theory]
    [InlineData(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float)]
    [InlineData(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double)]
    [InlineData(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal)]
    [InlineData(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float)]
    [InlineData(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Double)]
    [InlineData(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Decimal)]
    public void WithAllPrecisionCombinations_StoresPrecisionSettings(
      Compiler.IntegerPrecision intPrecision, Compiler.FloatPrecision floatPrecision)
    {
      var compiler = new Compiler(intPrecision, floatPrecision);
      var script = compiler.Compile("42");

      Assert.Equal(intPrecision, script.IntegerPrecision);
      Assert.Equal(floatPrecision, script.FloatPrecision);
    }

    [Fact]
    public void IntAndFloatMixed_UsesConfiguredFloatPrecision()
    {
      var compiler = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double);
      var script = compiler.Compile("10 + 2.5");

      script.Execute();

      Assert.Equal(Type.Double, script.ValueType);
      Assert.Equal(12.5, script.DoubleValue);
    }

    [Fact]
    public void LongAndDouble_UsesDouble()
    {
      var compiler = new Compiler(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Double);
      var script = compiler.Compile("3000000000 + 1.5");

      script.Execute();

      Assert.Equal(Type.Double, script.ValueType);
      Assert.Equal(3000000001.5, script.DoubleValue);
    }

    #endregion

    #region Value Accessor Tests

    [Fact]
    public void IntegerValue_ReturnsInt32()
    {
      var compiler = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      var script = compiler.Compile("42");
      script.Execute();

      Assert.Equal(42, script.IntegerValue);
    }

    [Fact]
    public void LongValue_ReturnsInt64()
    {
      var compiler = new Compiler(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float);
      var script = compiler.Compile("3000000000");
      script.Execute();

      Assert.Equal(3000000000L, script.LongValue);
    }

    [Fact]
    public void FloatValue_ReturnsFloat()
    {
      var compiler = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      var script = compiler.Compile("3.14");
      script.Execute();

      Assert.True(EpsilonScript.Math.IsNearlyEqual(3.14f, script.FloatValue));
    }

    [Fact]
    public void DoubleValue_ReturnsDouble()
    {
      var compiler = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double);
      var script = compiler.Compile("3.141592653589793");
      script.Execute();

      Assert.Equal(3.141592653589793, script.DoubleValue);
    }

    [Fact]
    public void DecimalValue_ReturnsDecimal()
    {
      var compiler = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal);
      var script = compiler.Compile("3.141592653589793238");
      script.Execute();

      Assert.Equal(3.141592653589793238m, script.DecimalValue);
    }

    #endregion

    #region Complex Expression Tests

    [Fact]
    public void WithLongPrecision_ComplexExpression()
    {
      var compiler = new Compiler(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float);
      var script = compiler.Compile("(5000000000 + 3000000000) * 2 / 4");

      script.Execute();

      Assert.Equal(Type.Long, script.ValueType);
      Assert.Equal(4000000000L, script.LongValue);
    }

    [Fact]
    public void WithDoublePrecision_ComplexExpression()
    {
      var compiler = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double);
      var script = compiler.Compile("(1.5 + 2.5) * 3.0 - 1.0");

      script.Execute();

      Assert.Equal(Type.Double, script.ValueType);
      Assert.Equal(11.0, script.DoubleValue);
    }

    [Fact]
    public void WithDecimalPrecision_HighPrecisionCalculation()
    {
      var compiler = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal);
      var script = compiler.Compile("0.1 + 0.1 + 0.1");

      script.Execute();

      Assert.Equal(Type.Decimal, script.ValueType);
      // Decimal maintains exact precision (no floating-point errors)
      Assert.Equal(0.3m, script.DecimalValue);
    }

    [Fact]
    public void WithLongAndDecimal_ComplexExpression()
    {
      var compiler = new Compiler(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Decimal);
      var script = compiler.Compile("5000000000 + 0.123456789");

      script.Execute();

      Assert.Equal(Type.Decimal, script.ValueType);
      Assert.Equal(5000000000.123456789m, script.DecimalValue);
    }

    #endregion

    #region Comparison Tests with Different Precisions

    [Fact]
    public void WithLongPrecision_ComparisonReturnsBoolean()
    {
      var compiler = new Compiler(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float);
      var script = compiler.Compile("5000000000 > 3000000000");

      script.Execute();

      Assert.Equal(Type.Boolean, script.ValueType);
      Assert.True(script.BooleanValue);
    }

    [Fact]
    public void WithDoublePrecision_ComparisonReturnsBoolean()
    {
      var compiler = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double);
      var script = compiler.Compile("3.14159265358979 > 3.14");

      script.Execute();

      Assert.Equal(Type.Boolean, script.ValueType);
      Assert.True(script.BooleanValue);
    }

    [Fact]
    public void WithDecimalPrecision_ExactComparison()
    {
      var compiler = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal);
      var script = compiler.Compile("0.1 + 0.2 == 0.3");

      script.Execute();

      Assert.Equal(Type.Boolean, script.ValueType);
      Assert.True(script.BooleanValue); // Decimal allows exact comparison
    }

    #endregion

    #region Default Constructor Test

    [Fact]
    public void Compiler_DefaultConstructor_UsesDefaultPrecisions()
    {
      var compiler = new Compiler(); // Default: Integer and Float
      var intScript = compiler.Compile("42");
      intScript.Execute();

      Assert.Equal(Compiler.IntegerPrecision.Integer, intScript.IntegerPrecision);
      Assert.Equal(Compiler.FloatPrecision.Float, intScript.FloatPrecision);

      var floatScript = compiler.Compile("3.14");
      floatScript.Execute();

      Assert.Equal(Type.Float, floatScript.ValueType);
    }

    #endregion
  }
}