using EpsilonScript.Function;
using EpsilonScript.Tests.TestInfrastructure;
using System;
using Xunit;

namespace EpsilonScript.Tests.ScriptSystem
{
  /// <summary>
  /// Tests for edge cases in function usage
  /// Covers deeply nested calls, special return values, complex names, and contextual functions
  /// </summary>
  [Trait("Category", "Integration")]
  [Trait("Component", "ScriptSystem")]
  [Trait("Priority", "Medium")]
  public class ScriptFunctionEdgeCaseTests : ScriptTestBase
  {
    #region Function Name Edge Cases

    [Fact]
    public void Function_NameWithManyDots_Works()
    {
      var func = CustomFunction.Create(
        "deeply.nested.namespace.utility.function",
        (int x) => x * 10);

      var result = CompileAndExecute(
        "deeply.nested.namespace.utility.function(5)",
        Compiler.Options.Immutable,
        extraFunctions: func);

      Assert.Equal(50, result.IntegerValue);
    }

    [Fact]
    public void Function_VeryLongName_Works()
    {
      var longName = new string('f', 200);
      var func = CustomFunction.Create(longName, (int x) => x);

      var result = CompileAndExecute($"{longName}(999)", Compiler.Options.Immutable,
        extraFunctions: func);

      Assert.Equal(999, result.IntegerValue);
    }

    [Fact]
    public void Function_SingleCharacterName_Works()
    {
      var func = CustomFunction.Create("f", (int x) => x * x);

      var result = CompileAndExecute("f(7)", Compiler.Options.Immutable,
        extraFunctions: func);

      Assert.Equal(49, result.IntegerValue);
    }

    #endregion

    #region Deeply Nested Function Calls

    [Fact]
    public void Function_Nested_2Levels_Works()
    {
      var add1 = CustomFunction.Create("add1", (int x) => x + 1);
      var mul2 = CustomFunction.Create("mul2", (int x) => x * 2);

      var result = CompileAndExecute("mul2(add1(5))", Compiler.Options.Immutable,
        extraFunctions: new[] { add1, mul2 });

      Assert.Equal(12, result.IntegerValue); // (5+1)*2 = 12
    }

    [Fact]
    public void Function_Nested_3Levels_Works()
    {
      var add1 = CustomFunction.Create("add1", (int x) => x + 1);
      var mul2 = CustomFunction.Create("mul2", (int x) => x * 2);
      var sub3 = CustomFunction.Create("sub3", (int x) => x - 3);

      var result = CompileAndExecute("sub3(mul2(add1(10)))", Compiler.Options.Immutable,
        extraFunctions: new[] { add1, mul2, sub3 });

      Assert.Equal(19, result.IntegerValue); // ((10+1)*2)-3 = 19
    }

    [Fact]
    public void Function_Nested_5Levels_Works()
    {
      var f1 = CustomFunction.Create("f1", (int x) => x + 1);
      var f2 = CustomFunction.Create("f2", (int x) => x * 2);
      var f3 = CustomFunction.Create("f3", (int x) => x - 3);
      var f4 = CustomFunction.Create("f4", (int x) => x + 4);
      var f5 = CustomFunction.Create("f5", (int x) => x * 5);

      var result = CompileAndExecute(
        "f5(f4(f3(f2(f1(10)))))",
        Compiler.Options.Immutable,
        extraFunctions: new[] { f1, f2, f3, f4, f5 });

      // ((((10+1)*2)-3)+4)*5 = ((22-3)+4)*5 = 23*5 = 115
      Assert.Equal(115, result.IntegerValue);
    }

    [Fact]
    public void Function_Nested_10Levels_Works()
    {
      var functions = new CustomFunction[10];
      for (var i = 0; i < 10; i++)
      {
        var level = i; // Capture for closure
        functions[i] = CustomFunction.Create(
          $"level{i}",
          (int x) => x + level);
      }

      var expr = "10";
      for (var i = 0; i < 10; i++)
      {
        expr = $"level{i}({expr})";
      }

      var result = CompileAndExecute(expr, Compiler.Options.Immutable,
        extraFunctions: functions);

      // 10 + 0 + 1 + 2 + 3 + 4 + 5 + 6 + 7 + 8 + 9 = 55
      Assert.Equal(55, result.IntegerValue);
    }

    #endregion

    #region Special Return Values

    [Fact]
    public void Function_ReturnsNaN_Works()
    {
      var func = CustomFunction.Create(
        "getNaN",
        () => float.NaN);

      var result = CompileAndExecute("getNaN()", Compiler.Options.Immutable,
        extraFunctions: func);

      Assert.True(float.IsNaN(result.FloatValue));
    }

    [Fact]
    public void Function_ReturnsInfinity_Works()
    {
      var func = CustomFunction.Create(
        "getInf",
        () => float.PositiveInfinity);

      var result = CompileAndExecute("getInf()", Compiler.Options.Immutable,
        extraFunctions: func);

      Assert.True(float.IsPositiveInfinity(result.FloatValue));
    }

    [Fact]
    public void Function_ReturnsNegativeInfinity_Works()
    {
      var func = CustomFunction.Create(
        "getNegInf",
        () => float.NegativeInfinity);

      var result = CompileAndExecute("getNegInf()", Compiler.Options.Immutable,
        extraFunctions: func);

      Assert.True(float.IsNegativeInfinity(result.FloatValue));
    }

    [Fact]
    public void Function_ReturnsZero_Works()
    {
      var func = CustomFunction.Create("zero", () => 0);

      var result = CompileAndExecute("zero()", Compiler.Options.Immutable,
        extraFunctions: func);

      Assert.Equal(0, result.IntegerValue);
    }

    [Fact]
    public void Function_ReturnsEmptyString_Works()
    {
      var func = CustomFunction.Create("empty", () => "");

      var result = CompileAndExecute("empty()", Compiler.Options.Immutable,
        extraFunctions: func);

      Assert.Equal("", result.StringValue);
    }

    #endregion

    #region Contextual Function Edge Cases

    [Fact]
    public void ContextualFunction_MissingVariable_ThrowsException()
    {
      var compiler = CreateCompiler();
      compiler.AddCustomFunction(
        CustomFunction.CreateContextual("needs", "required", (int x) => x));

      var script = compiler.Compile("needs()", Compiler.Options.Immutable);
      ErrorTestHelper.AssertRuntimeException(() => script.Execute(), "required");
    }

    [Fact]
    public void ContextualFunction_VariableInOverride_Works()
    {
      var func = CustomFunction.CreateContextual("readContext", "value", (int x) => x * 2);

      var overrideVars = Variables().WithInteger("value", 21).Build();
      var result = CompileAndExecute("readContext()",
        Compiler.Options.Immutable,
        null,
        overrideVars,
        func);

      Assert.Equal(42, result.IntegerValue);
    }

    [Fact]
    public void ContextualFunction_VariableChanges_BetweenExecutions()
    {
      var compiler = CreateCompiler();
      compiler.AddCustomFunction(
        CustomFunction.CreateContextual("double", "x", (int x) => x * 2));

      var variables = Variables().WithInteger("x", 10).Build();
      var script = compiler.Compile("double()", Compiler.Options.Immutable, variables);

      script.Execute();
      Assert.Equal(20, script.IntegerValue);

      variables["x"] = new VariableValue(25);
      script.Execute();
      Assert.Equal(50, script.IntegerValue);
    }

    [Fact]
    public void ContextualFunction_WithParameters_Works()
    {
      var func = CustomFunction.CreateContextual("addTo", "base",
        (int baseVal, int add) => baseVal + add);

      var variables = Variables().WithInteger("base", 100).Build();
      var result = CompileAndExecute("addTo(23)", Compiler.Options.Immutable,
        variables, null, func);

      Assert.Equal(123, result.IntegerValue);
    }

    #endregion

    #region Function with Complex Arguments

    [Fact]
    public void Function_ArgumentIsExpression_Works()
    {
      var func = CustomFunction.Create("square", (int x) => x * x);

      var result = CompileAndExecute("square(3 + 4)", Compiler.Options.Immutable,
        extraFunctions: func);

      Assert.Equal(49, result.IntegerValue); // (3+4)^2 = 49
    }

    [Fact]
    public void Function_ArgumentIsAnotherFunction_Works()
    {
      var double_func = CustomFunction.Create("double", (int x) => x * 2);
      var triple = CustomFunction.Create("triple", (int x) => x * 3);

      var result = CompileAndExecute("double(triple(5))", Compiler.Options.Immutable,
        extraFunctions: new[] { double_func, triple });

      Assert.Equal(30, result.IntegerValue); // (5*3)*2 = 30
    }

    [Fact]
    public void Function_MultipleArguments_AllExpressions_Works()
    {
      var func = CustomFunction.Create(
        "calc",
        (int a, int b, int c) => a + b * c);

      var result = CompileAndExecute(
        "calc(1 + 1, 2 + 2, 3 + 3)",
        Compiler.Options.Immutable,
        extraFunctions: func);

      Assert.Equal(26, result.IntegerValue); // 2 + 4*6 = 26
    }

    #endregion

    #region Built-in Function Edge Cases

    [Fact]
    public void BuiltIn_Sin_VeryLargeValue_Works()
    {
      var result = CompileAndExecute("sin(1000000.0)", Compiler.Options.Immutable);
      Assert.True(result.FloatValue >= -1.0f && result.FloatValue <= 1.0f);
    }

    [Fact]
    public void BuiltIn_Sqrt_Zero_ReturnsZero()
    {
      var result = CompileAndExecute("sqrt(0.0)", Compiler.Options.Immutable);
      AssertNearlyEqual(0.0f, result.FloatValue);
    }

    [Fact]
    public void BuiltIn_Pow_ZeroExponent_ReturnsOne()
    {
      var result = CompileAndExecute("pow(99.0, 0.0)", Compiler.Options.Immutable);
      AssertNearlyEqual(1.0f, result.FloatValue);
    }

    [Fact]
    public void BuiltIn_Abs_AlreadyPositive_Unchanged()
    {
      var result = CompileAndExecute("abs(42)", Compiler.Options.Immutable);
      Assert.Equal(42, result.IntegerValue);
    }

    [Fact]
    public void BuiltIn_Min_SameValues_ReturnsValue()
    {
      var result = CompileAndExecute("min(7, 7)", Compiler.Options.Immutable);
      Assert.Equal(7, result.IntegerValue);
    }

    [Fact]
    public void BuiltIn_Max_SameValues_ReturnsValue()
    {
      var result = CompileAndExecute("max(9, 9)", Compiler.Options.Immutable);
      Assert.Equal(9, result.IntegerValue);
    }

    [Fact]
    public void BuiltIn_Ifelse_BothBranches_SameType_Works()
    {
      var result = CompileAndExecute("ifelse(true, 10, 20)", Compiler.Options.Immutable);
      Assert.Equal(10, result.IntegerValue);
    }

    [Fact]
    public void BuiltIn_StringFunctions_Chained_Work()
    {
      var result = CompileAndExecute(
        "upper(lower(\"HeLLo\"))",
        Compiler.Options.Immutable);

      Assert.Equal("HELLO", result.StringValue);
    }

    #endregion
  }
}