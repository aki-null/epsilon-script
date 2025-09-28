using Xunit;
using EpsilonScript.Helper;

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
  }
}