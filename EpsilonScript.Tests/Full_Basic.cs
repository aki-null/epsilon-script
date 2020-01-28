using System.Collections.Generic;
using Xunit;

namespace EpsilonScript.Tests
{
  public class Full_Basic
  {
    [Fact]
    private void Full_Basic_Succeeds()
    {
      var compiler = new Compiler();
      var rootNode = compiler.Compile("10 + -2 * -(20.2 - 10)", Compiler.Options.Immutable);
      rootNode.Execute();
      Assert.True(Math.IsNearlyEqual(30.4f, rootNode.FloatValue));
    }

    [Fact]
    private void Full_VariableAssign_Succeeds()
    {
      var compiler = new Compiler();
      var variables = new Dictionary<string, VariableValue> {["val"] = new VariableValue(0.0f)};
      var rootNode = compiler.Compile("(val = 10 + -2 * -(20.2 - 10); val *= 2; val / 2) * 2 / 2",
        Compiler.Options.None, variables);
      rootNode.Execute();
      Assert.True(Math.IsNearlyEqual(30.4f, rootNode.FloatValue));
      Assert.True(Math.IsNearlyEqual(60.8f, variables["val"].FloatValue));
    }

    [Fact]
    private void Full_FunctionOverloadInteger_Succeeds()
    {
      var compiler = new Compiler();
      var variables = new Dictionary<string, VariableValue> {["val"] = new VariableValue(1.0f)};
      var script = compiler.Compile("ifelse(val <= 0, 200, 100)", Compiler.Options.None, variables);
      script.Execute();
      Assert.Equal(Type.Integer, script.ValueType);
      Assert.Equal(100, script.IntegerValue);
    }

    [Fact]
    private void Full_FunctionOverloadFloat_Succeeds()
    {
      var compiler = new Compiler();
      var variables = new Dictionary<string, VariableValue> {["val"] = new VariableValue(1.0f)};
      var rootNode = compiler.Compile("ifelse(val <= 0, 1.5, 100.2)", Compiler.Options.Immutable, variables);
      rootNode.Execute();
      Assert.Equal(Type.Float, rootNode.ValueType);
      Assert.True(Math.IsNearlyEqual(rootNode.FloatValue, 100.2f));
    }

    [Fact]
    private void Full_CompilerReuse_Succeeds()
    {
      var compiler = new Compiler();
      var script = compiler.Compile("0");
      script.Execute();
      Assert.Equal(Type.Integer, script.ValueType);
      Assert.Equal(0, script.IntegerValue);
      script = compiler.Compile("1");
      script.Execute();
      Assert.Equal(Type.Integer, script.ValueType);
      Assert.Equal(1, script.IntegerValue);
      script = compiler.Compile("2.0");
      script.Execute();
      Assert.Equal(Type.Float, script.ValueType);
      Assert.Equal(2.0f, script.IntegerValue);
    }
  }
}