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
      var rootNode = compiler.Compile("10 + -2 * -(20.2 - 10)");
      rootNode.Execute();
      Assert.True(Math.IsNearlyEqual(30.4f, rootNode.FloatValue));
    }

    [Fact]
    private void Full_VariableAssign_Succeeds()
    {
      var compiler = new Compiler();
      var variables = new Dictionary<string, VariableValue> {["val"] = new VariableValue(0.0f)};
      var rootNode = compiler.Compile("(val = 10 + -2 * -(20.2 - 10); val *= 2; val / 2) * 2 / 2", variables);
      rootNode.Execute();
      Assert.True(Math.IsNearlyEqual(30.4f, rootNode.FloatValue));
      Assert.True(Math.IsNearlyEqual(60.8f, variables["val"].FloatValue));
    }

    [Fact]
    private void Full_FunctionSin_Succeeds()
    {
      var compiler = new Compiler();
      var variables = new Dictionary<string, VariableValue> {["val"] = new VariableValue(1.0f)};
      var rootNode = compiler.Compile("ifelse(val <= 0, 200, 100)", variables);
      rootNode.Execute();
      Assert.True(Math.IsNearlyEqual(100.0f, rootNode.FloatValue));
    }
  }
}