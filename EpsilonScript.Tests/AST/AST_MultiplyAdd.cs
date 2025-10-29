using System;
using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;
using EpsilonScript.Tests.ScriptSystem;

namespace EpsilonScript.Tests.AST
{
  [Trait("Category", "Unit")]
  [Trait("Component", "AST")]
  public class AST_MultiplyAdd : ScriptTestBase
  {
    [Fact]
    public void MA_LeftMultiply_Integer_ProducesCorrectResult()
    {
      // Test: (2 * 3) + 4 = 10
      var vars = Variables()
        .WithInteger("a", 2)
        .WithInteger("b", 3)
        .WithInteger("c", 4)
        .Build();

      var result = CompileAndExecute("(a * b) + c", Compiler.Options.None, vars);

      Assert.Equal(10, result.IntegerValue);
    }

    [Fact]
    public void MA_RightMultiply_Integer_ProducesCorrectResult()
    {
      // Test: 4 + (2 * 3) = 10
      var vars = Variables()
        .WithInteger("a", 2)
        .WithInteger("b", 3)
        .WithInteger("c", 4)
        .Build();

      var result = CompileAndExecute("c + (a * b)", Compiler.Options.None, vars);

      Assert.Equal(10, result.IntegerValue);
    }

    [Fact]
    public void MA_LongType_ProducesCorrectResult()
    {
      var compiler = new Compiler(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float);
      var vars = new DictionaryVariableContainer();
      vars["a"] = new VariableValue(1000000000L);
      vars["b"] = new VariableValue(2L);
      vars["c"] = new VariableValue(5L);

      var script = compiler.Compile("(a * b) + c", Compiler.Options.None, vars);
      script.Execute();

      Assert.Equal(2000000005L, script.LongValue);
    }

    [Fact]
    public void MA_FloatType_ProducesCorrectResult()
    {
      var vars = Variables()
        .WithFloat("a", 2.5f)
        .WithFloat("b", 4.0f)
        .WithFloat("c", 1.5f)
        .Build();

      var result = CompileAndExecute("(a * b) + c", Compiler.Options.None, vars);

      AssertNearlyEqual(11.5f, result.FloatValue);
    }

    [Fact]
    public void MA_DoubleType_ProducesCorrectResult()
    {
      var compiler = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double);
      var vars = new DictionaryVariableContainer();
      vars["a"] = new VariableValue(2.5);
      vars["b"] = new VariableValue(4.0);
      vars["c"] = new VariableValue(1.5);

      var script = compiler.Compile("(a * b) + c", Compiler.Options.None, vars);
      script.Execute();

      Assert.Equal(11.5, script.DoubleValue, 5);
    }

    [Fact]
    public void MA_DecimalType_ProducesCorrectResult()
    {
      var compiler = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal);
      var vars = new DictionaryVariableContainer();
      vars["a"] = new VariableValue(2.5m);
      vars["b"] = new VariableValue(4.0m);
      vars["c"] = new VariableValue(1.5m);

      var script = compiler.Compile("(a * b) + c", Compiler.Options.None, vars);
      script.Execute();

      Assert.Equal(11.5m, script.DecimalValue);
    }

    [Fact]
    public void MA_TypePromotion_IntToFloat()
    {
      // (int * int) + float should promote to float
      var vars = Variables()
        .WithInteger("a", 2)
        .WithInteger("b", 3)
        .WithFloat("c", 1.5f)
        .Build();

      var result = CompileAndExecute("(a * b) + c", Compiler.Options.None, vars);

      Assert.Equal(Type.Float, result.Type);
      AssertNearlyEqual(7.5f, result.FloatValue);
    }

    [Fact]
    public void MA_TypePromotion_FloatInMultiply()
    {
      // (float * int) + int should promote to float
      var vars = Variables()
        .WithFloat("a", 2.5f)
        .WithInteger("b", 4)
        .WithInteger("c", 1)
        .Build();

      var result = CompileAndExecute("(a * b) + c", Compiler.Options.None, vars);

      Assert.Equal(Type.Float, result.Type);
      AssertNearlyEqual(11.0f, result.FloatValue);
    }

    [Fact]
    public void MA_ConstantFolding_AllConstants()
    {
      // (2 * 3) + 4 should be folded to constant 10
      var result = CompileAndExecute("(2 * 3) + 4");

      Assert.Equal(10, result.IntegerValue);
      Assert.True(result.IsConstant);
    }

    [Fact]
    public void MA_NestedExpression_InnerVariable()
    {
      // ((a * b) + c) + d
      var vars = Variables()
        .WithInteger("a", 2)
        .WithInteger("b", 3)
        .WithInteger("c", 4)
        .WithInteger("d", 5)
        .Build();

      var result = CompileAndExecute("((a * b) + c) + d", Compiler.Options.None, vars);

      Assert.Equal(15, result.IntegerValue); // (2*3)+4+5 = 6+4+5 = 15
    }

    [Fact]
    public void MA_NestedExpression_OuterVariable()
    {
      // d + ((a * b) + c)
      var vars = Variables()
        .WithInteger("a", 2)
        .WithInteger("b", 3)
        .WithInteger("c", 4)
        .WithInteger("d", 5)
        .Build();

      var result = CompileAndExecute("d + ((a * b) + c)", Compiler.Options.None, vars);

      Assert.Equal(15, result.IntegerValue); // 5+(2*3+4) = 5+10 = 15
    }

    [Fact]
    public void MA_MultipleInExpression()
    {
      // (a * b) + (c * d) - the right side multiply should be fused
      var vars = Variables()
        .WithInteger("a", 2)
        .WithInteger("b", 3)
        .WithInteger("c", 4)
        .WithInteger("d", 5)
        .Build();

      var result = CompileAndExecute("(a * b) + (c * d)", Compiler.Options.None, vars);

      Assert.Equal(26, result.IntegerValue); // (2*3)+(4*5) = 6+20 = 26
    }

    [Fact]
    public void NonMA_WrongOrder_MultiplyAfterAdd()
    {
      // (a + b) * c should not create FMA
      var vars = Variables()
        .WithInteger("a", 2)
        .WithInteger("b", 3)
        .WithInteger("c", 4)
        .Build();

      var result = CompileAndExecute("(a + b) * c", Compiler.Options.None, vars);

      Assert.Equal(20, result.IntegerValue); // (2+3)*4 = 5*4 = 20
    }

    [Fact]
    public void MA_SubtractAddend_Integer()
    {
      // (a * b) - c should be fused
      var vars = Variables()
        .WithInteger("a", 2)
        .WithInteger("b", 3)
        .WithInteger("c", 4)
        .Build();

      var result = CompileAndExecute("(a * b) - c", Compiler.Options.None, vars);

      Assert.Equal(2, result.IntegerValue); // (2*3)-4 = 6-4 = 2
    }

    [Fact]
    public void MA_SubtractFromAddend_Integer()
    {
      // c - (a * b) should be fused
      var vars = Variables()
        .WithInteger("a", 2)
        .WithInteger("b", 3)
        .WithInteger("c", 10)
        .Build();

      var result = CompileAndExecute("c - (a * b)", Compiler.Options.None, vars);

      Assert.Equal(4, result.IntegerValue); // 10-(2*3) = 10-6 = 4
    }

    [Fact]
    public void MA_SubtractAddend_Float()
    {
      // (a * b) - c with floats
      var vars = Variables()
        .WithFloat("a", 2.5f)
        .WithFloat("b", 4.0f)
        .WithFloat("c", 3.5f)
        .Build();

      var result = CompileAndExecute("(a * b) - c", Compiler.Options.None, vars);

      AssertNearlyEqual(6.5f, result.FloatValue); // (2.5*4.0)-3.5 = 10.0-3.5 = 6.5
    }

    [Fact]
    public void MA_SubtractFromAddend_Float()
    {
      // c - (a * b) with floats
      var vars = Variables()
        .WithFloat("a", 2.5f)
        .WithFloat("b", 4.0f)
        .WithFloat("c", 15.0f)
        .Build();

      var result = CompileAndExecute("c - (a * b)", Compiler.Options.None, vars);

      AssertNearlyEqual(5.0f, result.FloatValue); // 15.0-(2.5*4.0) = 15.0-10.0 = 5.0
    }

    [Fact]
    public void MA_SubtractAddend_NegativeResult()
    {
      // (a * b) - c where result is negative
      var vars = Variables()
        .WithInteger("a", 2)
        .WithInteger("b", 3)
        .WithInteger("c", 10)
        .Build();

      var result = CompileAndExecute("(a * b) - c", Compiler.Options.None, vars);

      Assert.Equal(-4, result.IntegerValue); // (2*3)-10 = 6-10 = -4
    }

    [Fact]
    public void MA_SubtractFromAddend_NegativeResult()
    {
      // c - (a * b) where result is negative
      var vars = Variables()
        .WithInteger("a", 5)
        .WithInteger("b", 3)
        .WithInteger("c", 10)
        .Build();

      var result = CompileAndExecute("c - (a * b)", Compiler.Options.None, vars);

      Assert.Equal(-5, result.IntegerValue); // 10-(5*3) = 10-15 = -5
    }

    [Fact]
    public void MA_SubtractAddend_TypePromotion()
    {
      // (int * int) - float should promote to float
      var vars = Variables()
        .WithInteger("a", 3)
        .WithInteger("b", 4)
        .WithFloat("c", 2.5f)
        .Build();

      var result = CompileAndExecute("(a * b) - c", Compiler.Options.None, vars);

      Assert.Equal(Type.Float, result.Type);
      AssertNearlyEqual(9.5f, result.FloatValue); // (3*4)-2.5 = 12-2.5 = 9.5
    }

    [Fact]
    public void MA_SubtractFromAddend_TypePromotion()
    {
      // float - (int * int) should promote to float
      var vars = Variables()
        .WithInteger("a", 3)
        .WithInteger("b", 4)
        .WithFloat("c", 15.5f)
        .Build();

      var result = CompileAndExecute("c - (a * b)", Compiler.Options.None, vars);

      Assert.Equal(Type.Float, result.Type);
      AssertNearlyEqual(3.5f, result.FloatValue); // 15.5-(3*4) = 15.5-12 = 3.5
    }

    [Fact]
    public void MA_SubtractAddend_WithFunctionCall()
    {
      // (abs(a) * b) - c
      var vars = Variables()
        .WithInteger("a", -5)
        .WithInteger("b", 3)
        .WithInteger("c", 10)
        .Build();

      var result = CompileAndExecute("(abs(a) * b) - c", Compiler.Options.None, vars);

      Assert.Equal(5, result.IntegerValue); // (abs(-5)*3)-10 = (5*3)-10 = 15-10 = 5
    }

    [Fact]
    public void MA_SubtractFromAddend_WithFunctionCall()
    {
      // c - (abs(a) * b)
      var vars = Variables()
        .WithInteger("a", -5)
        .WithInteger("b", 3)
        .WithInteger("c", 20)
        .Build();

      var result = CompileAndExecute("c - (abs(a) * b)", Compiler.Options.None, vars);

      Assert.Equal(5, result.IntegerValue); // 20-(abs(-5)*3) = 20-(5*3) = 20-15 = 5
    }

    [Fact]
    public void MA_SubtractAddend_ConstantFolding()
    {
      // (2 * 3) - 4 should be folded to constant 2
      var result = CompileAndExecute("(2 * 3) - 4");

      Assert.Equal(2, result.IntegerValue);
      Assert.True(result.IsConstant);
    }

    [Fact]
    public void MA_SubtractFromAddend_ConstantFolding()
    {
      // 10 - (2 * 3) should be folded to constant 4
      var result = CompileAndExecute("10 - (2 * 3)");

      Assert.Equal(4, result.IntegerValue);
      Assert.True(result.IsConstant);
    }

    [Fact]
    public void MA_WithFunctionCall()
    {
      // (abs(a) * b) + c
      var vars = Variables()
        .WithInteger("a", -2)
        .WithInteger("b", 3)
        .WithInteger("c", 4)
        .Build();

      var result = CompileAndExecute("(abs(a) * b) + c", Compiler.Options.None, vars);

      Assert.Equal(10, result.IntegerValue); // (abs(-2)*3)+4 = (2*3)+4 = 10
    }

    [Fact]
    public void MA_ComplexExpression()
    {
      // x * (a + b) + c - the multiply doesn't have simple operands
      var vars = Variables()
        .WithInteger("x", 2)
        .WithInteger("a", 3)
        .WithInteger("b", 4)
        .WithInteger("c", 5)
        .Build();

      var result = CompileAndExecute("x * (a + b) + c", Compiler.Options.None, vars);

      Assert.Equal(19, result.IntegerValue); // 2*(3+4)+5 = 2*7+5 = 14+5 = 19
    }

    [Fact]
    public void MA_ZeroMultiplier()
    {
      // (0 * b) + c should work correctly
      var vars = Variables()
        .WithInteger("a", 0)
        .WithInteger("b", 999)
        .WithInteger("c", 5)
        .Build();

      var result = CompileAndExecute("(a * b) + c", Compiler.Options.None, vars);

      Assert.Equal(5, result.IntegerValue); // (0*999)+5 = 0+5 = 5
    }

    [Fact]
    public void MA_NegativeNumbers()
    {
      // (-2 * 3) + 10 = -6 + 10 = 4
      var vars = Variables()
        .WithInteger("a", -2)
        .WithInteger("b", 3)
        .WithInteger("c", 10)
        .Build();

      var result = CompileAndExecute("(a * b) + c", Compiler.Options.None, vars);

      Assert.Equal(4, result.IntegerValue);
    }

    [Fact]
    public void MA_LargeNumbers()
    {
      // Test with large integers that don't overflow
      var compiler = new Compiler(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float);
      var vars = new DictionaryVariableContainer();
      vars["a"] = new VariableValue(1000000L);
      vars["b"] = new VariableValue(1000L);
      vars["c"] = new VariableValue(123456L);

      var script = compiler.Compile("(a * b) + c", Compiler.Options.None, vars);
      script.Execute();

      Assert.Equal(1000123456L, script.LongValue);
    }

    [Fact]
    public void MA_FloatPrecision()
    {
      // Test floating point arithmetic precision
      var compiler = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double);
      var vars = new DictionaryVariableContainer();
      vars["a"] = new VariableValue(0.1);
      vars["b"] = new VariableValue(0.2);
      vars["c"] = new VariableValue(0.3);

      var script = compiler.Compile("(a * b) + c", Compiler.Options.None, vars);
      script.Execute();

      // 0.1 * 0.2 + 0.3 = 0.02 + 0.3 = 0.32
      Assert.Equal(0.32, script.DoubleValue, 10);
    }
  }
}