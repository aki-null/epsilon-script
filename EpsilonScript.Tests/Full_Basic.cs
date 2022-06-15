using System.Collections.Generic;
using EpsilonScript.Helper;
using Xunit;

namespace EpsilonScript.Tests
{
  public class Full_Basic
  {
    [Fact]
    private void Full_Basic_Succeeds()
    {
      var compiler = new Compiler();
      var script = compiler.Compile("10 + -2 * -(20.2 - 10)", Compiler.Options.Immutable);
      script.Execute();
      Assert.True(Math.IsNearlyEqual(30.4f, script.FloatValue));
    }

    [Fact]
    private void Full_VariableAssign_Float_Succeeds()
    {
      var compiler = new Compiler();
      var variables = new DictionaryVariableContainer { ["val".GetUniqueIdentifier()] = new VariableValue(0.0f) };
      var script = compiler.Compile("(val = 10 + -2 * -(20.2 - 10); val *= 2; val / 2) * 2 / 2",
        Compiler.Options.None, variables);
      script.Execute();
      Assert.True(Math.IsNearlyEqual(30.4f, script.FloatValue));
      Assert.True(Math.IsNearlyEqual(60.8f, variables["val".GetUniqueIdentifier()].FloatValue));
    }

    [Fact]
    private void Full_VariableAssign_Int_Succeeds()
    {
      var compiler = new Compiler();
      var valId = "val".GetUniqueIdentifier();
      var variables = new DictionaryVariableContainer { [valId] = new VariableValue(0) };
      var script = compiler.Compile("(val = 10 + -2 * -(20 - 10); val *= 2; val / 2) * 2 / 2",
        Compiler.Options.None, variables);
      script.Execute();
      Assert.Equal(30, script.IntegerValue);
      Assert.Equal(60, variables[valId].IntegerValue);
    }

    [Fact]
    private void Full_VariableAssign_Bool_Succeeds()
    {
      var compiler = new Compiler();
      var valId = "val".GetUniqueIdentifier();
      var variables = new DictionaryVariableContainer { [valId] = new VariableValue(false) };
      var script = compiler.Compile("(val = 30 > 20); ifelse(val, 30, 50)",
        Compiler.Options.None, variables);
      script.Execute();
      Assert.Equal(30, script.IntegerValue);
      Assert.True(variables[valId].BooleanValue);
    }

    [Fact]
    private void Full_FunctionOverloadInteger_Succeeds()
    {
      var compiler = new Compiler();
      var variables = new DictionaryVariableContainer { ["val".GetUniqueIdentifier()] = new VariableValue(1.0f) };
      var script = compiler.Compile("ifelse(val <= 0, 200, 100)", Compiler.Options.None, variables);
      script.Execute();
      Assert.Equal(Type.Integer, script.ValueType);
      Assert.Equal(100, script.IntegerValue);
    }

    [Fact]
    private void Full_FunctionOverloadFloat_Succeeds()
    {
      var compiler = new Compiler();
      var variables = new DictionaryVariableContainer { ["v".GetUniqueIdentifier()] = new VariableValue(1.0f) };
      var rootNode = compiler.Compile("ifelse(v <= 0, 1.5, 100.2)", Compiler.Options.Immutable, variables);
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