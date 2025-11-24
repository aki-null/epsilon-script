using Xunit;

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
      Assert.Equal(Type.Integer, result.Type);
      Assert.True(result.IsPrecomputable);
      Assert.Equal(18, result.IntegerValue);
    }

    [Fact]
    public void ConstantFloatExpression_ReturnsExpectedValue()
    {
      var result = CompileAndExecute("10 + -2 * -(20.2 - 10)", Compiler.Options.Immutable);
      Assert.Equal(Type.Float, result.Type);
      Assert.True(result.IsPrecomputable);
      AssertNearlyEqual(30.4f, result.FloatValue);
    }

    [Fact]
    public void MixedIntegerFloatExpression_ReturnsFloat()
    {
      var result = CompileAndExecute("(10 + 2.5) / 2", Compiler.Options.Immutable);
      Assert.Equal(Type.Float, result.Type);
      Assert.True(result.IsPrecomputable);
      AssertNearlyEqual(6.25f, result.FloatValue);
    }

    [Fact]
    public void ModuloExpression_ReturnsRemainder()
    {
      var result = CompileAndExecute("23 % 7", Compiler.Options.Immutable);
      Assert.Equal(Type.Integer, result.Type);
      Assert.Equal(2, result.IntegerValue);
    }

    [Fact]
    public void FloatModuloExpression_ReturnsRemainder()
    {
      var result = CompileAndExecute("10.5 % 4", Compiler.Options.Immutable);
      Assert.Equal(Type.Float, result.Type);
      AssertNearlyEqual(2.5f, result.FloatValue);
    }

    [Fact]
    public void StringConcatenationWithNumbers_ReturnsString()
    {
      var result = CompileAndExecute("\"Hello \" + 10 + \"!\"", Compiler.Options.Immutable);
      Assert.Equal(Type.String, result.Type);
      Assert.Equal("Hello 10!", result.StringValue);
    }

    [Fact]
    public void IntegerResult_StringValue_ReturnsStringRepresentation()
    {
      var result = CompileAndExecute("42");
      Assert.Equal(Type.Integer, result.Type);
      Assert.Equal(42, result.IntegerValue);
      Assert.Equal("42", result.StringValue);
    }

    [Fact]
    public void IntegerExpression_StringValue_ReturnsStringRepresentation()
    {
      var result = CompileAndExecute("42 + 8");
      Assert.Equal(Type.Integer, result.Type);
      Assert.Equal(50, result.IntegerValue);
      Assert.Equal("50", result.StringValue);
    }

    [Fact]
    public void FloatResult_StringValue_ReturnsStringRepresentation()
    {
      var result = CompileAndExecute("3.14");
      Assert.Equal(Type.Float, result.Type);
      AssertNearlyEqual(3.14f, result.FloatValue);
      Assert.Equal("3.14", result.StringValue);
    }

    [Fact]
    public void BooleanResult_StringValue_ReturnsStringRepresentation()
    {
      var result = CompileAndExecute("5 > 3");
      Assert.Equal(Type.Boolean, result.Type);
      Assert.True(result.BooleanValue);
      Assert.Equal("True", result.StringValue);
    }

    [Fact]
    public void BooleanFalse_StringValue_ReturnsStringRepresentation()
    {
      var result = CompileAndExecute("5 < 3");
      Assert.Equal(Type.Boolean, result.Type);
      Assert.False(result.BooleanValue);
      Assert.Equal("False", result.StringValue);
    }

    [Fact]
    public void NegativeInteger_StringValue_ReturnsStringRepresentation()
    {
      var result = CompileAndExecute("-123");
      Assert.Equal(Type.Integer, result.Type);
      Assert.Equal(-123, result.IntegerValue);
      Assert.Equal("-123", result.StringValue);
    }
  }
}