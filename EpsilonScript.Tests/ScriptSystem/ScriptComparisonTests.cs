using Xunit;
using Type = EpsilonScript.Type;

namespace EpsilonScript.Tests.ScriptSystem
{
  [Trait("Category", "Integration")]
  [Trait("Component", "ScriptSystem")]
  public class ScriptComparisonTests : ScriptTestBase
  {
    [Theory]
    [InlineData("10 < 20", true)]
    [InlineData("20 <= 20", true)]
    [InlineData("25 > 20", true)]
    [InlineData("25 >= 30", false)]
    [InlineData("30 == 30", true)]
    [InlineData("30 != 30", false)]
    public void IntegerComparisons_ReturnExpectedResults(string expression, bool expected)
    {
      var result = CompileAndExecute(expression, Compiler.Options.Immutable);
      Assert.Equal(Type.Boolean, result.ValueType);
      Assert.Equal(expected, result.BooleanValue);
    }

    [Theory]
    [InlineData("10.0 == 10", true)]
    [InlineData("10.0 != 10", false)]
    [InlineData("10.5 < 11", true)]
    [InlineData("11 > 12.5", false)]
    public void MixedNumberComparisons_ReturnExpectedResults(string expression, bool expected)
    {
      var result = CompileAndExecute(expression, Compiler.Options.Immutable);
      Assert.Equal(Type.Boolean, result.ValueType);
      Assert.Equal(expected, result.BooleanValue);
    }

    [Fact]
    public void StringComparisons_ReturnExpectedResults()
    {
      var equalsResult = CompileAndExecute("\"hello\" == \"hello\"", Compiler.Options.Immutable);
      Assert.True(equalsResult.BooleanValue);

      var notEqualsResult = CompileAndExecute("\"hello\" != \"world\"", Compiler.Options.Immutable);
      Assert.True(notEqualsResult.BooleanValue);
    }

    [Fact]
    public void BooleanComparisons_ReturnExpectedResults()
    {
      var equalResult = CompileAndExecute("true == (5 > 2)", Compiler.Options.Immutable);
      Assert.True(equalResult.BooleanValue);

      var notEqualResult = CompileAndExecute("false != (5 > 2)", Compiler.Options.Immutable);
      Assert.True(notEqualResult.BooleanValue);
    }
  }
}
