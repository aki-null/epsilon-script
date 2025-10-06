using System;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.AST
{
  [Trait("Category", "Integration")]
  [Trait("Component", "AST")]
  public class AST_VariableAutoConversion
  {
    [Fact]
    public void AST_Variable_AutoConverts_FloatVariableToDouble_WhenCompilerUsesDouble()
    {
      // Compiler configured for Double precision
      var compiler = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double);

      // Variable created with Float type
      var variables = new DictionaryVariableContainer
      {
        ["x"] = new VariableValue(1.5f) // Float variable
      };

      // Script uses the variable in arithmetic
      var script = compiler.Compile("x + 0.123456789012345", Compiler.Options.None, variables);
      script.Execute();

      // Should perform double-precision arithmetic
      var expected = 1.5 + 0.123456789012345;
      Assert.Equal(expected, script.DoubleValue, precision: 14);
    }

    [Fact]
    public void AST_Variable_AutoConverts_DoubleVariableToFloat_WhenCompilerUsesFloat()
    {
      // Compiler configured for Float precision
      var compiler = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);

      // Variable created with Double type
      var variables = new DictionaryVariableContainer
      {
        ["x"] = new VariableValue(1.5) // Double variable
      };

      // Script uses the variable in arithmetic
      var script = compiler.Compile("x + 0.1234567", Compiler.Options.None, variables);
      script.Execute();

      // Should perform float-precision arithmetic (result will be truncated to float)
      var expected = 1.5f + 0.1234567f;
      Assert.Equal(expected, script.FloatValue, precision: 6);
    }

    [Fact]
    public void AST_Variable_AutoConverts_DecimalVariableToDouble_WhenCompilerUsesDouble()
    {
      // Compiler configured for Double precision
      var compiler = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double);

      // Variable created with Decimal type
      var variables = new DictionaryVariableContainer
      {
        ["x"] = new VariableValue(1.234567890123456789m) // Decimal variable
      };

      // Script uses the variable in arithmetic
      var script = compiler.Compile("x + 0.5", Compiler.Options.None, variables);
      script.Execute();

      // Should convert to double and perform double-precision arithmetic
      var expected = 1.234567890123456789 + 0.5;
      Assert.Equal(expected, script.DoubleValue, precision: 14);
    }

    [Fact]
    public void AST_Variable_AutoConverts_IntegerVariableToLong_WhenCompilerUsesLong()
    {
      // Compiler configured for Long precision
      var compiler = new Compiler(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float);

      // Variable created with Integer type
      var variables = new DictionaryVariableContainer
      {
        ["x"] = new VariableValue(42) // Integer variable
      };

      // Script uses the variable in arithmetic
      var script = compiler.Compile("x + 1000000000000", Compiler.Options.None, variables);
      script.Execute();

      // Should convert to long and perform long-precision arithmetic
      long expected = 42L + 1000000000000L;
      Assert.Equal(expected, script.LongValue);
    }

    [Fact]
    public void AST_Variable_AutoConverts_LongVariableToInteger_WhenCompilerUsesInteger()
    {
      // Compiler configured for Integer precision
      var compiler = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);

      // Variable created with Long type
      var variables = new DictionaryVariableContainer
      {
        ["x"] = new VariableValue(42L) // Long variable
      };

      // Script uses the variable in arithmetic
      var script = compiler.Compile("x + 100", Compiler.Options.None, variables);
      script.Execute();

      // Should convert to int and perform int-precision arithmetic
      int expected = 42 + 100;
      Assert.Equal(expected, script.IntegerValue);
    }

    [Fact]
    public void AST_Variable_AutoConverts_MixedPrecisionVariables_InComplexExpression()
    {
      // Compiler configured for Double and Long precision
      var compiler = new Compiler(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Double);

      // Variables with mismatched types
      var variables = new DictionaryVariableContainer
      {
        ["a"] = new VariableValue(1.5f), // Float (should convert to Double)
        ["b"] = new VariableValue(2.5m), // Decimal (should convert to Double)
        ["c"] = new VariableValue(100), // Integer (should convert to Long)
        ["d"] = new VariableValue(200L) // Long (already correct)
      };

      // Complex expression mixing all variables
      var script = compiler.Compile("a + b + c + d", Compiler.Options.None, variables);
      script.Execute();

      // All should be converted to appropriate precision
      double expected = 1.5 + 2.5 + 100.0 + 200.0;
      Assert.Equal(expected, script.DoubleValue, precision: 14);
    }

    [Fact]
    public void AST_Variable_AutoConverts_FloatToDouble_InComparison()
    {
      // Compiler configured for Double precision
      var compiler = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double);

      // Variable with Float type (will be converted to Double for comparison)
      var variables = new DictionaryVariableContainer
      {
        ["x"] = new VariableValue(1.5f) // Float variable
      };

      // Comparison with double literal - should convert variable to double
      var script = compiler.Compile("x == 1.5", Compiler.Options.None, variables);
      script.Execute();

      // Float 1.5f converts cleanly to Double 1.5, should compare equal
      Assert.True(script.BooleanValue);
    }

    [Fact]
    public void AST_Variable_AutoConverts_ComparesMixedPrecisionVariables()
    {
      // Compiler configured for Double precision
      var compiler = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double);

      // Variables with different precision
      var variables = new DictionaryVariableContainer
      {
        ["a"] = new VariableValue(2.5f), // Float (will convert to Double)
        ["b"] = new VariableValue(1.5m) // Decimal (will convert to Double)
      };

      // Comparison between mismatched variable types
      var script = compiler.Compile("a > b", Compiler.Options.None, variables);
      script.Execute();

      // Both converted to Double: 2.5 > 1.5 = true
      Assert.True(script.BooleanValue);
    }

    [Fact]
    public void AST_Variable_AutoConverts_WithAssignmentOperations()
    {
      // Compiler configured for Double precision
      var compiler = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double);

      // Variable with Float type
      var variables = new DictionaryVariableContainer
      {
        ["x"] = new VariableValue(0.0f) // Float variable
      };

      // Assignment should respect variable's actual type (Float)
      var script = compiler.Compile("x = 1.234567890123", Compiler.Options.None, variables);
      script.Execute();

      // Variable type remains Float, but value is converted when written
      Assert.Equal(Type.Float, variables["x"].Type);
      // The value is clamped to float precision
      Assert.Equal(1.2345679f, variables["x"].FloatValue, precision: 6);
    }

    [Fact]
    public void AST_Variable_AutoConverts_DoesNotAffectBooleanVariables()
    {
      // Compiler configured with any precision
      var compiler = new Compiler(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Double);

      // Boolean variable should not be affected by precision settings
      var variables = new DictionaryVariableContainer
      {
        ["flag"] = new VariableValue(true)
      };

      var script = compiler.Compile("flag", Compiler.Options.None, variables);
      script.Execute();

      Assert.True(script.BooleanValue);
      Assert.Equal(Type.Boolean, variables["flag"].Type);
    }

    [Fact]
    public void AST_Variable_AutoConverts_DoesNotAffectStringVariables()
    {
      // Compiler configured with any precision
      var compiler = new Compiler(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Decimal);

      // String variable should not be affected by precision settings
      var variables = new DictionaryVariableContainer
      {
        ["text"] = new VariableValue("hello")
      };

      var script = compiler.Compile("text", Compiler.Options.None, variables);
      script.Execute();

      Assert.Equal("hello", script.StringValue);
      Assert.Equal(Type.String, variables["text"].Type);
    }

    [Fact]
    public void AST_Variable_AutoConverts_PreservesOriginalVariableType()
    {
      // Compiler configured for Double precision
      var compiler = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double);

      // Variable with Float type
      var floatVariable = new VariableValue(1.5f);
      var variables = new DictionaryVariableContainer
      {
        ["x"] = floatVariable
      };

      // Use variable in expression
      var script = compiler.Compile("x + 0.5", Compiler.Options.None, variables);
      script.Execute();

      // Original variable type should remain Float
      Assert.Equal(Type.Float, floatVariable.Type);
      Assert.Equal(1.5f, floatVariable.FloatValue, precision: 6);
    }

    [Fact]
    public void AST_Variable_AutoConverts_DecimalToDouble_InArithmeticChain()
    {
      // Compiler configured for Double precision
      var compiler = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double);

      // Variable with Decimal type (higher precision than Double)
      var variables = new DictionaryVariableContainer
      {
        ["x"] = new VariableValue(1.234567890123456789012345678m)
      };

      // Chain of arithmetic operations
      var script = compiler.Compile("x * 2.0 + x * 3.0", Compiler.Options.None, variables);
      script.Execute();

      // Should use double precision throughout (decimal precision lost during conversion)
      double xAsDouble = 1.234567890123456789012345678;
      double expected = xAsDouble * 2.0 + xAsDouble * 3.0;
      Assert.Equal(expected, script.DoubleValue, precision: 14);
    }
  }
}