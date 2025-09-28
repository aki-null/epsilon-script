using System;
using Xunit;
using EpsilonScript.Function;

namespace EpsilonScript.Tests.ScriptSystem
{
  [Trait("Category", "Integration")]
  [Trait("Component", "ScriptSystem")]
  public class ScriptFunctionTests : ScriptTestBase
  {
    [Fact]
    public void BuiltInSin_ReturnsExpectedValue()
    {
      var result = CompileAndExecute("sin(0.0)", Compiler.Options.Immutable);
      Assert.Equal(Type.Float, result.ValueType);
      AssertNearlyEqual(0.0f, result.FloatValue);
    }

    [Fact]
    public void BuiltInCos_ReturnsExpectedValue()
    {
      var result = CompileAndExecute("cos(0.0)", Compiler.Options.Immutable);
      Assert.Equal(Type.Float, result.ValueType);
      AssertNearlyEqual(1.0f, result.FloatValue);
    }

    [Fact]
    public void BuiltInAbsOverloads_ReturnCorrectTypes()
    {
      var intResult = CompileAndExecute("abs(-5)", Compiler.Options.Immutable);
      Assert.Equal(Type.Integer, intResult.ValueType);
      Assert.Equal(5, intResult.IntegerValue);

      var floatResult = CompileAndExecute("abs(-5.5)", Compiler.Options.Immutable);
      Assert.Equal(Type.Float, floatResult.ValueType);
      AssertNearlyEqual(5.5f, floatResult.FloatValue);
    }

    [Fact]
    public void BuiltInSqrt_ReturnsExpectedValue()
    {
      var result = CompileAndExecute("sqrt(9.0)", Compiler.Options.Immutable);
      Assert.Equal(Type.Float, result.ValueType);
      AssertNearlyEqual(3.0f, result.FloatValue);
    }

    [Fact]
    public void BuiltInIfElse_SelectsBranchesByType()
    {
      var intResult = CompileAndExecute("ifelse(true, 10, 20)", Compiler.Options.Immutable);
      Assert.Equal(Type.Integer, intResult.ValueType);
      Assert.Equal(10, intResult.IntegerValue);

      var floatResult = CompileAndExecute("ifelse(false, 1.5, 2.5)", Compiler.Options.Immutable);
      Assert.Equal(Type.Float, floatResult.ValueType);
      AssertNearlyEqual(2.5f, floatResult.FloatValue);

      var stringResult = CompileAndExecute("ifelse(true, \"left\", \"right\")", Compiler.Options.Immutable);
      Assert.Equal(Type.String, stringResult.ValueType);
      Assert.Equal("left", stringResult.StringValue);
    }

    [Fact]
    public void BuiltInMin_EvaluatesCompositeArguments()
    {
      var result = CompileAndExecute("min(1 + 2, 3 + 4)", Compiler.Options.Immutable);
      Assert.Equal(Type.Integer, result.ValueType);
      Assert.Equal(3, result.IntegerValue);
    }

    [Fact]
    public void BuiltInMax_HandlesMixedNumericArguments()
    {
      var result = CompileAndExecute("max(1.5, 2)", Compiler.Options.Immutable);
      Assert.Equal(Type.Float, result.ValueType);
      AssertNearlyEqual(2.0f, result.FloatValue);
    }

    [Fact]
    public void BuiltInStringHelpers_ReturnExpectedValues()
    {
      var lower = CompileAndExecute("lower(\"HeLLo\")", Compiler.Options.Immutable);
      Assert.Equal("hello", lower.StringValue);

      var upper = CompileAndExecute("upper(\"HeLLo\")", Compiler.Options.Immutable);
      Assert.Equal("HELLO", upper.StringValue);

      var length = CompileAndExecute("len(\"Hello\")", Compiler.Options.Immutable);
      Assert.Equal(Type.Integer, length.ValueType);
      Assert.Equal(5, length.IntegerValue);
    }

    [Fact]
    public void BuiltInPow_ReturnsExponentiation()
    {
      var result = CompileAndExecute("pow(2, 3)", Compiler.Options.Immutable);
      Assert.Equal(Type.Float, result.ValueType);
      AssertNearlyEqual(8.0f, result.FloatValue);
    }

    [Fact]
    public void CustomFunctionOverloads_ResolveByArgumentTypes()
    {
      var compiler = CreateCompiler();
      compiler.AddCustomFunction(CustomFunction.Create("pick", (int value) => value + 1));
      compiler.AddCustomFunction(CustomFunction.Create("pick", (float value) => value + 0.5f));

      var intScript = compiler.Compile("pick(10)");
      intScript.Execute();
      Assert.Equal(Type.Integer, intScript.ValueType);
      Assert.Equal(11, intScript.IntegerValue);

      var floatScript = compiler.Compile("pick(10.0)");
      floatScript.Execute();
      Assert.Equal(Type.Float, floatScript.ValueType);
      AssertNearlyEqual(10.5f, floatScript.FloatValue);
    }

    [Fact]
    public void TupleArguments_CallCustomFunctionWithMultipleParameters()
    {
      var (function, counter) = TestFunctions.CreateTupleProbe("sum");
      var compiler = CreateCompiler(function);
      var script = compiler.Compile("sum(1, 2, 3)");
      script.Execute();
      Assert.Equal(Type.Float, script.ValueType);
      AssertNearlyEqual(6.0f, script.FloatValue);
      Assert.Equal(1, counter.Count);
    }

    [Fact]
    public void CustomFunctionFloatFallback_IsSelectedWhenIntegerOverloadMissing()
    {
      var compiler = CreateCompiler();
      compiler.AddCustomFunction(CustomFunction.Create("echo", (float value) => value + 0.25f));

      var script = compiler.Compile("echo(1)");
      script.Execute();

      Assert.Equal(Type.Float, script.ValueType);
      AssertNearlyEqual(1.25f, script.FloatValue);
    }

    [Fact]
    public void CustomFunctionConstnessMismatch_ThrowsArgumentException()
    {
      var compiler = CreateCompiler();
      compiler.AddCustomFunction(CustomFunction.Create("probe", (int value) => value, true));

      var exception = Assert.Throws<ArgumentException>(() =>
        compiler.AddCustomFunction(CustomFunction.Create("probe", (float value) => value, false)));

      Assert.Contains("constness", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CustomFunctionDuplicateSignature_ThrowsRuntimeException()
    {
      var compiler = CreateCompiler();
      compiler.AddCustomFunction(CustomFunction.Create("dup", (int value) => value));

      Assert.Throws<RuntimeException>(() =>
        compiler.AddCustomFunction(CustomFunction.Create("dup", (int value) => value * 2)));
    }

    [Fact]
    public void FiveArgumentCustomFunction_ReceivesAllArguments()
    {
      var counter = new TestFunctions.CallCounter();
      var fiveArgFunction = CustomFunction.Create("sum5", (float a, float b, float c, float d, float e) =>
      {
        counter.Increment();
        return a + b + c + d + e;
      });

      var compiler = CreateCompiler(fiveArgFunction);
      var script = compiler.Compile("sum5(1, 2, 3, 4, 5)");
      script.Execute();

      Assert.Equal(Type.Float, script.ValueType);
      AssertNearlyEqual(15.0f, script.FloatValue);
      Assert.Equal(1, counter.Count);
    }
  }
}