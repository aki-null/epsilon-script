using Xunit;
using Type = EpsilonScript.Type;

namespace EpsilonScript.Tests.ScriptSystem
{
  [Trait("Category", "Integration")]
  [Trait("Component", "ScriptSystem")]
  public class ScriptEvaluationTests : ScriptTestBase
  {
    [Fact]
    public void ConstantIntegerExpression_ReturnsExpectedValue()
    {
      var result = CompileAndExecute("(1 + 2 + 3 * 2) * 2", Compiler.Options.Immutable);
      Assert.Equal(Type.Integer, result.ValueType);
      Assert.True(result.IsConstant);
      Assert.Equal(18, result.IntegerValue);
    }

    [Fact]
    public void ConstantFloatExpression_ReturnsExpectedValue()
    {
      var result = CompileAndExecute("10 + -2 * -(20.2 - 10)", Compiler.Options.Immutable);
      Assert.Equal(Type.Float, result.ValueType);
      Assert.True(result.IsConstant);
      AssertNearlyEqual(30.4f, result.FloatValue);
    }

    [Fact]
    public void MixedIntegerFloatExpression_ReturnsFloat()
    {
      var result = CompileAndExecute("(10 + 2.5) / 2", Compiler.Options.Immutable);
      Assert.Equal(Type.Float, result.ValueType);
      Assert.True(result.IsConstant);
      AssertNearlyEqual(6.25f, result.FloatValue);
    }

    [Fact]
    public void ModuloExpression_ReturnsRemainder()
    {
      var result = CompileAndExecute("23 % 7", Compiler.Options.Immutable);
      Assert.Equal(Type.Integer, result.ValueType);
      Assert.Equal(2, result.IntegerValue);
    }

    [Fact]
    public void FloatModuloExpression_ReturnsRemainder()
    {
      var result = CompileAndExecute("10.5 % 4", Compiler.Options.Immutable);
      Assert.Equal(Type.Float, result.ValueType);
      AssertNearlyEqual(2.5f, result.FloatValue);
    }

    [Fact]
    public void StringConcatenationWithNumbers_ReturnsString()
    {
      var result = CompileAndExecute("\"Hello \" + 10 + \"!\"", Compiler.Options.Immutable);
      Assert.Equal(Type.String, result.ValueType);
      Assert.Equal("Hello 10!", result.StringValue);
    }
  }
}
