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

    [Fact]
    public void ZeroParameterFunction_WithParentheses_ExecutesCorrectly()
    {
      var getAnswer = CustomFunction.Create("getAnswer", () => 42);
      var getPi = CustomFunction.Create("getPi", () => 3.14159f);
      var getGreeting = CustomFunction.Create("getGreeting", () => "Hello");
      var isReady = CustomFunction.Create("isReady", () => true);

      // Test integer return
      var intResult = CompileAndExecute("getAnswer()", Compiler.Options.None, null, null, getAnswer);
      Assert.Equal(Type.Integer, intResult.ValueType);
      Assert.Equal(42, intResult.IntegerValue);

      // Test float return
      var floatResult = CompileAndExecute("getPi()", Compiler.Options.None, null, null, getPi);
      Assert.Equal(Type.Float, floatResult.ValueType);
      AssertNearlyEqual(3.14159f, floatResult.FloatValue);

      // Test string return
      var stringResult = CompileAndExecute("getGreeting()", Compiler.Options.None, null, null, getGreeting);
      Assert.Equal(Type.String, stringResult.ValueType);
      Assert.Equal("Hello", stringResult.StringValue);

      // Test boolean return
      var boolResult = CompileAndExecute("isReady()", Compiler.Options.None, null, null, isReady);
      Assert.Equal(Type.Boolean, boolResult.ValueType);
      Assert.True(boolResult.BooleanValue);
    }

    [Fact]
    public void ZeroParameterFunction_InExpression_ExecutesCorrectly()
    {
      var getTwo = CustomFunction.Create("getTwo", () => 2);
      var getThree = CustomFunction.Create("getThree", () => 3);

      // Test zero-parameter functions in arithmetic expressions
      var result1 = CompileAndExecute("getTwo() + getThree()", Compiler.Options.None, null, null, getTwo, getThree);
      Assert.Equal(Type.Integer, result1.ValueType);
      Assert.Equal(5, result1.IntegerValue);

      // Test zero-parameter function with regular parameter function
      var doubleFunc = CustomFunction.Create("double", (int x) => x * 2);
      var result2 = CompileAndExecute("double(getTwo())", Compiler.Options.None, null, null, getTwo, doubleFunc);
      Assert.Equal(Type.Integer, result2.ValueType);
      Assert.Equal(4, result2.IntegerValue);
    }

    [Fact]
    public void AddCustomFunctionRange_WithNullCollection_ThrowsArgumentNullException()
    {
      var compiler = CreateCompiler();
      Assert.Throws<ArgumentNullException>(() => compiler.AddCustomFunctionRange(null));
    }

    [Fact]
    public void AddCustomFunctionRange_WithEmptyCollection_DoesNotThrow()
    {
      var compiler = CreateCompiler();
      var emptyFunctions = new CustomFunction[] { };
      compiler.AddCustomFunctionRange(emptyFunctions);
    }

    [Fact]
    public void AddCustomFunctionRange_WithMultipleFunctions_AddsAllFunctions()
    {
      var compiler = CreateCompiler();
      var functions = new[]
      {
        CustomFunction.Create("add10", (int x) => x + 10),
        CustomFunction.Create("multiply2", (int x) => x * 2),
        CustomFunction.Create("square", (float x) => x * x)
      };

      compiler.AddCustomFunctionRange(functions);

      var addScript = compiler.Compile("add10(5)");
      addScript.Execute();
      Assert.Equal(Type.Integer, addScript.ValueType);
      Assert.Equal(15, addScript.IntegerValue);

      var multiplyScript = compiler.Compile("multiply2(7)");
      multiplyScript.Execute();
      Assert.Equal(Type.Integer, multiplyScript.ValueType);
      Assert.Equal(14, multiplyScript.IntegerValue);

      var squareScript = compiler.Compile("square(3.0)");
      squareScript.Execute();
      Assert.Equal(Type.Float, squareScript.ValueType);
      AssertNearlyEqual(9.0f, squareScript.FloatValue);
    }

    [Fact]
    public void AddCustomFunctionRange_WithOverloads_CreatesProperOverloads()
    {
      var compiler = CreateCompiler();
      var functions = new[]
      {
        CustomFunction.Create("test", (int x) => x + 1),
        CustomFunction.Create("test", (float x) => x + 0.5f),
        CustomFunction.Create("test", (string x) => x + "!")
      };

      compiler.AddCustomFunctionRange(functions);

      var intScript = compiler.Compile("test(5)");
      intScript.Execute();
      Assert.Equal(Type.Integer, intScript.ValueType);
      Assert.Equal(6, intScript.IntegerValue);

      var floatScript = compiler.Compile("test(5.0)");
      floatScript.Execute();
      Assert.Equal(Type.Float, floatScript.ValueType);
      AssertNearlyEqual(5.5f, floatScript.FloatValue);

      var stringScript = compiler.Compile("test(\"hello\")");
      stringScript.Execute();
      Assert.Equal(Type.String, stringScript.ValueType);
      Assert.Equal("hello!", stringScript.StringValue);
    }

    [Fact]
    public void AddCustomFunctionRange_MixedWithIndividualAdditions_WorksCorrectly()
    {
      var compiler = CreateCompiler();

      compiler.AddCustomFunction(CustomFunction.Create("existing", (int x) => x * 100));

      var rangeFunctions = new[]
      {
        CustomFunction.Create("range1", (int x) => x + 1),
        CustomFunction.Create("range2", (float x) => x * 2.0f)
      };
      compiler.AddCustomFunctionRange(rangeFunctions);

      compiler.AddCustomFunction(CustomFunction.Create("added_after", (string s) => s.ToUpper()));

      var existingScript = compiler.Compile("existing(3)");
      existingScript.Execute();
      Assert.Equal(300, existingScript.IntegerValue);

      var range1Script = compiler.Compile("range1(5)");
      range1Script.Execute();
      Assert.Equal(6, range1Script.IntegerValue);

      var range2Script = compiler.Compile("range2(3.5)");
      range2Script.Execute();
      AssertNearlyEqual(7.0f, range2Script.FloatValue);

      var afterScript = compiler.Compile("added_after(\"test\")");
      afterScript.Execute();
      Assert.Equal("TEST", afterScript.StringValue);
    }

    [Fact]
    public void PeriodInFunctionNames_EvaluatesCorrectly()
    {
      var utilDouble = CustomFunction.Create("util.double", (int x) => x * 2);
      var stringConcat = CustomFunction.Create("string.concat", (string a, string b) => a + b);

      var result1 = CompileAndExecute("util.double(5)", extraFunctions: utilDouble);
      Assert.Equal(Type.Integer, result1.ValueType);
      Assert.Equal(10, result1.IntegerValue);

      var result2 = CompileAndExecute("string.concat(\"Hello\", \" World\")", extraFunctions: stringConcat);
      Assert.Equal(Type.String, result2.ValueType);
      Assert.Equal("Hello World", result2.StringValue);
    }

    [Fact]
    public void ComplexPeriodIdentifiers_WorkCorrectly()
    {
      var mathSquare = CustomFunction.Create("math.square", (float x) => x * x);
      var variables = Variables()
        .WithFloat("math.pi.value", 3.14159f)
        .Build();

      var result = CompileAndExecute("math.square(math.pi.value)", variables: variables, extraFunctions: mathSquare);

      Assert.Equal(Type.Float, result.ValueType);
      // Calculate the expected value precisely: 3.14159f * 3.14159f = 9.869589f
      AssertNearlyEqual(9.869589f, result.FloatValue);
    }
  }
}