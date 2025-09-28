using EpsilonScript.Function;
using EpsilonScript.Tests.TestInfrastructure;
using Xunit;

namespace EpsilonScript.Tests.ScriptSystem
{
  [Trait("Category", "Integration")]
  [Trait("Component", "ScriptSystem")]
  public class ScriptErrorTests : ScriptTestBase
  {
    [Fact]
    public void UndefinedVariable_ThrowsRuntimeException()
    {
      var compiler = CreateCompiler();
      var script = compiler.Compile("missing + 1");
      ErrorTestHelper.AssertRuntimeException(() => script.Execute(), "Undefined variable");
    }

    [Fact]
    public void UndefinedFunction_ThrowsParserException()
    {
      var compiler = CreateCompiler();
      ErrorTestHelper.AssertParserException(() => compiler.Compile("foo(1)", Compiler.Options.Immutable), "foo");
    }

    [Fact]
    public void MissingFunctionOverload_ThrowsRuntimeException()
    {
      var compiler = CreateCompiler();
      compiler.AddCustomFunction(CustomFunction.Create("pick", (bool value) => value));
      var script = compiler.Compile("pick(1)");
      ErrorTestHelper.AssertRuntimeException(() => script.Execute(), "function");
    }

    [Fact]
    public void InvalidArgumentCount_ThrowsRuntimeException()
    {
      var compiler = CreateCompiler();
      compiler.AddCustomFunction(CustomFunction.Create("probe", (int value) => value));
      var script = compiler.Compile("probe(1, 2)");
      ErrorTestHelper.AssertRuntimeException(() => script.Execute(), "function");
    }

    [Fact]
    public void StringComparedToNumber_ThrowsRuntimeException()
    {
      var compiler = CreateCompiler();
      ErrorTestHelper.AssertRuntimeException(() => compiler.Compile("\"hello\" == 1", Compiler.Options.Immutable),
        "comparison");
    }

    [Fact]
    public void ArithmeticOnBoolean_ThrowsRuntimeException()
    {
      var compiler = CreateCompiler();
      ErrorTestHelper.AssertRuntimeException(() => compiler.Compile("true + 1", Compiler.Options.Immutable), "Boolean");
    }

    [Fact]
    public void BooleanOperationOnNumber_ThrowsRuntimeException()
    {
      var compiler = CreateCompiler();
      ErrorTestHelper.AssertRuntimeException(() => compiler.Compile("1 && true", Compiler.Options.Immutable), null);
    }

    [Fact]
    public void AssigningFloatToBooleanVariable_ThrowsRuntimeException()
    {
      var variables = Variables().WithBoolean("flag", false).Build();
      var compiler = CreateCompiler();
      var script = compiler.Compile("flag = 1.0", Compiler.Options.None, variables);
      ErrorTestHelper.AssertRuntimeException(() => script.Execute(), null);
    }

    [Fact]
    public void AssignmentRequiresVariableLeftHandSide()
    {
      var compiler = CreateCompiler();
      var script = compiler.Compile("1 = 2");
      ErrorTestHelper.AssertRuntimeException(() => script.Execute(), "assignment");
    }

    [Fact]
    public void NegatingNonBoolean_ThrowsRuntimeException()
    {
      var compiler = CreateCompiler();
      ErrorTestHelper.AssertRuntimeException(() => compiler.Compile("!1", Compiler.Options.Immutable), null);
    }

    [Fact]
    public void TupleComparison_ThrowsRuntimeException()
    {
      var compiler = CreateCompiler();
      ErrorTestHelper.AssertRuntimeException(() => compiler.Compile("(1, 2) == (1, 2)", Compiler.Options.Immutable),
        null);
    }

    [Theory]
    [InlineData("\"hello\" - \"world\"")]
    [InlineData("\"hello\" * \"world\"")]
    [InlineData("\"hello\" / \"world\"")]
    [InlineData("\"hello\" % \"world\"")]
    public void StringArithmeticOperations_ThrowsRuntimeException(string expression)
    {
      var compiler = CreateCompiler();

      ErrorTestHelper.AssertRuntimeException(() => compiler.Compile(expression, Compiler.Options.Immutable),
        "String operations only support concatenation (+), not");
    }

    [Theory]
    [InlineData("\"apple\" < \"banana\"")]
    [InlineData("\"apple\" <= \"banana\"")]
    [InlineData("\"apple\" > \"banana\"")]
    [InlineData("\"apple\" >= \"banana\"")]
    public void StringOrderingComparisons_ThrowsRuntimeException(string expression)
    {
      var compiler = CreateCompiler();

      ErrorTestHelper.AssertRuntimeException(() => compiler.Compile(expression, Compiler.Options.Immutable),
        "Cannot find values to perform arithmetic comparision on non numeric types");
    }
  }
}