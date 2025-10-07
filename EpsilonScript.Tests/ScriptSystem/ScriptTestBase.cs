using System;
using EpsilonScript.Function;
using Xunit;

namespace EpsilonScript.Tests.ScriptSystem
{
  public abstract class ScriptTestBase
  {
    protected static Compiler CreateCompiler(params CustomFunction[] extraFunctions)
    {
      var compiler = new Compiler();
      if (extraFunctions != null)
      {
        foreach (var function in extraFunctions)
        {
          compiler.AddCustomFunction(function);
        }
      }

      return compiler;
    }

    protected static CompiledScript Compile(string source, Compiler.Options options = Compiler.Options.None,
      DictionaryVariableContainer variables = null, params CustomFunction[] extraFunctions)
    {
      var compiler = CreateCompiler(extraFunctions);
      return compiler.Compile(source, options, variables);
    }

    protected static ScriptExecutionResult CompileAndExecute(string source,
      Compiler.Options options = Compiler.Options.None,
      DictionaryVariableContainer variables = null,
      IVariableContainer overrideVariables = null,
      params CustomFunction[] extraFunctions)
    {
      var compiler = CreateCompiler(extraFunctions);
      var script = compiler.Compile(source, options, variables);
      script.Execute(overrideVariables);
      return new ScriptExecutionResult(script, variables);
    }

    protected static ScriptExecutionResult CompileAndExecute(ReadOnlyMemory<char> source,
      Compiler.Options options = Compiler.Options.None,
      DictionaryVariableContainer variables = null,
      IVariableContainer overrideVariables = null,
      params CustomFunction[] extraFunctions)
    {
      var compiler = CreateCompiler(extraFunctions);
      var script = compiler.Compile(source, options, variables);
      script.Execute(overrideVariables);
      return new ScriptExecutionResult(script, variables);
    }

    protected static void AssertNearlyEqual(float expected, float actual)
    {
      Assert.True(EpsilonScript.Math.IsNearlyEqual(expected, actual),
        $"Expected {expected} to be nearly equal to {actual}");
    }

    protected static VariableContainerBuilder Variables()
    {
      return new VariableContainerBuilder();
    }

    protected readonly struct ScriptExecutionResult
    {
      public ScriptExecutionResult(CompiledScript script, DictionaryVariableContainer variables)
      {
        Script = script;
        Variables = variables;
      }

      public CompiledScript Script { get; }
      public DictionaryVariableContainer Variables { get; }

      public Type Type => Script.ValueType;
      public bool IsConstant => Script.IsConstant;
      public int IntegerValue => Script.IntegerValue;
      public float FloatValue => Script.FloatValue;
      public bool BooleanValue => Script.BooleanValue;
      public string StringValue => Script.StringValue;
    }
  }
}