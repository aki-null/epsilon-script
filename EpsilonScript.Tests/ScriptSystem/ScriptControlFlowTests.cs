using Xunit;

namespace EpsilonScript.Tests.ScriptSystem
{
  [Trait("Category", "Integration")]
  [Trait("Component", "ScriptSystem")]
  public class ScriptControlFlowTests : ScriptTestBase
  {
    [Fact]
    public void BooleanAnd_ShortCircuitsWhenLeftIsFalse()
    {
      var (function, counter) = TestFunctions.CreateBooleanProbe("probe");
      var compiler = CreateCompiler(function);
      var script = compiler.Compile("probe(false) && probe(true)");
      script.Execute();
      Assert.Equal(Type.Boolean, script.ValueType);
      Assert.False(script.BooleanValue);
      Assert.Equal(1, counter.Count);
    }

    [Fact]
    public void BooleanOr_ShortCircuitsWhenLeftIsTrue()
    {
      var (function, counter) = TestFunctions.CreateBooleanProbe("probe");
      var compiler = CreateCompiler(function);
      var script = compiler.Compile("probe(true) || probe(false)");
      script.Execute();
      Assert.Equal(Type.Boolean, script.ValueType);
      Assert.True(script.BooleanValue);
      Assert.Equal(1, counter.Count);
    }

    [Fact]
    public void LogicalNegation_ReturnsOppositeBoolean()
    {
      var result = CompileAndExecute("!(10 > 5)");
      Assert.Equal(Type.Boolean, result.ValueType);
      Assert.False(result.BooleanValue);
    }
  }
}