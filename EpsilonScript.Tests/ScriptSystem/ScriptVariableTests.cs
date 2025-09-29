using System;
using Xunit;

namespace EpsilonScript.Tests.ScriptSystem
{
  [Trait("Category", "Integration")]
  [Trait("Component", "ScriptSystem")]
  public class ScriptVariableTests : ScriptTestBase
  {
    [Fact]
    public void VariableRead_UsesCompilationContainer()
    {
      var variables = Variables().WithInteger("val", 5).Build();
      var result = CompileAndExecute("val * 3", Compiler.Options.None, variables);
      Assert.Equal(Type.Integer, result.ValueType);
      Assert.Equal(15, result.IntegerValue);
    }

    [Fact]
    public void VariableRead_UsesOverrideContainerWhenProvided()
    {
      var compileTimeVariables = Variables().WithInteger("val", 2).Build();
      var overrideVariables = Variables().WithInteger("val", 5).Build();
      var script = Compile("val * 3", Compiler.Options.None, compileTimeVariables);
      script.Execute(overrideVariables);
      Assert.Equal(Type.Integer, script.ValueType);
      Assert.Equal(15, script.IntegerValue);
    }

    [Theory]
    [InlineData("val = 42", 0, 42)]
    [InlineData("val += 5", 10, 15)]
    [InlineData("val -= 7", 10, 3)]
    [InlineData("val *= 2", 7, 14)]
    [InlineData("val /= 4", 20, 5)]
    public void IntegerAssignments_UpdateVariable(string expression, int startValue, int expected)
    {
      var variables = Variables().WithInteger("val", startValue).Build();
      var result = CompileAndExecute(expression, Compiler.Options.None, variables);
      Assert.Equal(Type.Integer, result.ValueType);
      Assert.Equal(expected, result.IntegerValue);
      Assert.Equal(expected, variables["val"].IntegerValue);
    }

    [Theory]
    [InlineData("val = 8.5", 1.5f, 8.5f)]
    [InlineData("val += 0.5", 8.0f, 8.5f)]
    [InlineData("val -= 2.25", 8.5f, 6.25f)]
    [InlineData("val *= 2", 3.25f, 6.5f)]
    [InlineData("val /= 2", 6.5f, 3.25f)]
    public void FloatAssignments_UpdateVariable(string expression, float startValue, float expected)
    {
      var variables = Variables().WithFloat("val", startValue).Build();
      var result = CompileAndExecute(expression, Compiler.Options.None, variables);
      Assert.Equal(Type.Float, result.ValueType);
      AssertNearlyEqual(expected, result.FloatValue);
      AssertNearlyEqual(expected, variables["val"].FloatValue);
    }

    [Fact]
    public void BooleanAssignmentFromExpression_UpdatesVariable()
    {
      var variables = Variables().WithBoolean("flag", false).Build();
      var result = CompileAndExecute("flag = 30 > 20", Compiler.Options.None, variables);
      Assert.Equal(Type.Boolean, result.ValueType);
      Assert.True(result.BooleanValue);
      Assert.True(variables["flag"].BooleanValue);
    }

    [Fact]
    public void SequenceExpression_ReturnsLastValueAndMutates()
    {
      var variables = Variables().WithInteger("val", 0).Build();
      var result = CompileAndExecute("(val = 1; val *= 5; val + 2)", Compiler.Options.None, variables);
      Assert.Equal(Type.Integer, result.ValueType);
      Assert.Equal(7, result.IntegerValue);
      Assert.Equal(5, variables["val"].IntegerValue);
    }

    [Fact]
    public void StringVariableRead_UsesCompilationContainer()
    {
      var variables = Variables().WithString("message", "Hello World").Build();
      var result = CompileAndExecute("message", Compiler.Options.None, variables);
      Assert.Equal(Type.String, result.ValueType);
      Assert.Equal("Hello World", result.StringValue);
    }

    [Fact]
    public void StringVariableRead_UsesOverrideContainerWhenProvided()
    {
      var compileTimeVariables = Variables().WithString("message", "Compile Time").Build();
      var overrideVariables = Variables().WithString("message", "Runtime Override").Build();
      var script = Compile("message", Compiler.Options.None, compileTimeVariables);
      script.Execute(overrideVariables);
      Assert.Equal(Type.String, script.ValueType);
      Assert.Equal("Runtime Override", script.StringValue);
    }

    [Fact]
    public void StringAssignment_UpdatesVariable()
    {
      var variables = Variables().WithString("text", "original").Build();
      var result = CompileAndExecute("text = \"updated\"", Compiler.Options.None, variables);
      Assert.Equal(Type.String, result.ValueType);
      Assert.Equal("updated", result.StringValue);
      Assert.Equal("updated", variables["text"].StringValue);
    }

    [Fact]
    public void StringVariableWithEmptyString_Works()
    {
      var variables = Variables().WithString("empty", "").Build();
      var result = CompileAndExecute("empty", Compiler.Options.None, variables);
      Assert.Equal(Type.String, result.ValueType);
      Assert.Equal("", result.StringValue);
    }

    [Fact]
    public void StringVariableWithSpecialCharacters_Works()
    {
      var variables = Variables().WithString("special", "Hello\nWorld\t!").Build();
      var result = CompileAndExecute("special", Compiler.Options.None, variables);
      Assert.Equal(Type.String, result.ValueType);
      Assert.Equal("Hello\nWorld\t!", result.StringValue);
    }

    [Theory]
    [InlineData("123", 123)]
    [InlineData("0", 0)]
    [InlineData("-456", -456)]
    [InlineData("  789  ", 789)]
    [InlineData("+42", 42)]
    public void StringVariableAsInteger_ParsesCorrectly(string stringValue, int expected)
    {
      var variable = new VariableValue(stringValue);
      Assert.Equal(Type.String, variable.Type);
      Assert.Equal(expected, variable.IntegerValue);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("12.34")]
    [InlineData("")]
    [InlineData("123abc")]
    [InlineData("2147483648")] // int.MaxValue + 1
    public void StringVariableAsInteger_ThrowsWhenInvalid(string stringValue)
    {
      var variable = new VariableValue(stringValue);
      Assert.Equal(Type.String, variable.Type);
      Assert.Throws<InvalidCastException>(() => variable.IntegerValue);
    }

    [Fact]
    public void StringVariableIntegerSetter_ConvertsToString()
    {
      var variable = new VariableValue("original");
      variable.IntegerValue = 42;
      Assert.Equal(Type.String, variable.Type);
      Assert.Equal("42", variable.StringValue);
    }
  }
}