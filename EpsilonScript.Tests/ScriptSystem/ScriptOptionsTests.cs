using System;
using Xunit;

namespace EpsilonScript.Tests.ScriptSystem
{
  [Trait("Category", "Integration")]
  [Trait("Component", "ScriptSystem")]
  public class ScriptOptionsTests : ScriptTestBase
  {
    [Fact]
    public void ImmutableOption_DisallowsAssignments()
    {
      var variables = Variables().WithInteger("val", 0).Build();
      var compiler = CreateCompiler();
      Assert.Throws<ParserException>(() => compiler.Compile("val = 5", Compiler.Options.Immutable, variables));
    }

    [Fact]
    public void CompilerReuse_AllowsMultipleCompilations()
    {
      var compiler = CreateCompiler();

      var scriptA = compiler.Compile("0", Compiler.Options.Immutable);
      scriptA.Execute();
      Assert.Equal(Type.Integer, scriptA.ValueType);
      Assert.Equal(0, scriptA.IntegerValue);

      var scriptB = compiler.Compile("1.5", Compiler.Options.Immutable);
      scriptB.Execute();
      Assert.Equal(Type.Float, scriptB.ValueType);
      AssertNearlyEqual(1.5f, scriptB.FloatValue);
    }

    [Fact]
    public void ConstantScripts_CacheExecutionResults()
    {
      var (function, counter) = TestFunctions.CreateFloatProbe("probe", isConstant: true);
      var compiler = CreateCompiler(function);
      var script = compiler.Compile("probe(10.0)", Compiler.Options.Immutable);

      script.Execute();
      script.Execute();

      Assert.Equal(1, counter.Count);
      Assert.True(script.IsConstant);
      AssertNearlyEqual(10.0f, script.FloatValue);
    }

    [Fact]
    public void VariableDependentScripts_ReexecuteOnSubsequentRuns()
    {
      var (function, counter) = TestFunctions.CreateFloatProbe("probe");
      var compiler = CreateCompiler(function);
      var variables = Variables().WithFloat("val", 1.0f).Build();
      var script = compiler.Compile("probe(val)", Compiler.Options.None, variables);

      script.Execute();
      AssertNearlyEqual(1.0f, script.FloatValue);

      variables["val"].FloatValue = 2.0f;
      script.Execute();
      AssertNearlyEqual(2.0f, script.FloatValue);
      Assert.Equal(2, counter.Count);
      Assert.False(script.IsConstant);
    }

    [Fact]
    public void CompileWithReadOnlyMemorySource_ProducesSameResult()
    {
      var memory = "21".AsMemory();
      var result = CompileAndExecute(memory, Compiler.Options.Immutable);
      Assert.Equal(Type.Integer, result.ValueType);
      Assert.Equal(21, result.IntegerValue);
    }
  }
}