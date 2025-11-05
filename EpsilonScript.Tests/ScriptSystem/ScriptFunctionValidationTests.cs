using EpsilonScript.Function;
using Xunit;

namespace EpsilonScript.Tests.ScriptSystem
{
  [Trait("Category", "Integration")]
  [Trait("Component", "ScriptSystem")]
  public class ScriptFunctionValidationTests : ScriptTestBase
  {
    #region Compile-Time Validation for Constant Expressions

    [Fact]
    public void FunctionCall_WithLiteralTypeMismatch_ThrowsAtCompileTime()
    {
      var compiler = CreateCompiler();
      compiler.AddCustomFunction(CustomFunction.Create("getConfig", (string s, int i) => i > 0));

      // User's exact example: float literal (2.0) when int expected
      var ex = Assert.Throws<ParserException>(() => compiler.Compile("getConfig('test', 2.0)"));

      Assert.Contains("getConfig", ex.Message);
      Assert.Contains("(string, float)", ex.Message); // What was provided
      Assert.Contains("(string, int)", ex.Message); // What was expected
    }

    [Fact]
    public void FunctionCall_WithStringConcatenation_ValidatesAtCompileTime()
    {
      var compiler = CreateCompiler();
      compiler.AddCustomFunction(CustomFunction.Create("process", (string s, int i) => i));

      // Expression "test" + "2" is optimized to StringNode("test2") before validation
      var ex = Assert.Throws<ParserException>(() => compiler.Compile("process('test' + '2', 2.0)"));

      Assert.Contains("process", ex.Message);
      Assert.Contains("(string, float)", ex.Message);
    }

    [Fact]
    public void FunctionCall_WithArithmeticExpression_ValidatesAtCompileTime()
    {
      var compiler = CreateCompiler();
      compiler.AddCustomFunction(CustomFunction.Create("calc", (int a, int b) => a + b));

      // 1 + 2 is optimized to IntegerNode(3) before validation
      var ex = Assert.Throws<ParserException>(() => compiler.Compile("calc(1 + 2, 3.0)"));

      Assert.Contains("calc", ex.Message);
      Assert.Contains("(int, float)", ex.Message);
      Assert.Contains("(int, int)", ex.Message);
    }

    [Fact]
    public void FunctionCall_WithNestedFunction_WorksCorrectly()
    {
      var compiler = CreateCompiler();
      compiler.AddCustomFunction(CustomFunction.Create("double", (int x) => x * 2));

      // min(1, 2) is optimized to IntegerNode(1) before validation
      // This should work since min(1,2) → int and double(int) exists
      var script = compiler.Compile("double(min(1, 2))");
      script.Execute();

      Assert.Equal(2, script.IntegerValue); // min(1,2)=1, double(1)=2
    }

    #endregion

    #region Runtime Validation for Variable Expressions

    [Fact]
    public void FunctionCall_WithVariable_DefersToRuntime()
    {
      var compiler = CreateCompiler();
      compiler.AddCustomFunction(CustomFunction.Create("process", (int x) => x * 2));

      var variables = Variables().WithFloat("x", 2.0f).Build();

      // Should compile successfully (variable type unknown at compile time)
      var script = compiler.Compile("process(x)", Compiler.Options.Immutable, variables);

      // Should throw at runtime when type mismatch discovered
      var ex = Assert.Throws<RuntimeException>(() => script.Execute());
      Assert.Contains("process", ex.Message);
    }

    [Fact]
    public void FunctionCall_WithMixedLiteralAndVariable_DetectsCompileTimeFailure()
    {
      var compiler = CreateCompiler();
      compiler.AddCustomFunction(CustomFunction.Create("calc", (int a, int b) => a + b));

      var variables = Variables().WithInteger("x", 5).Build();

      // With new validation: concrete param (float) doesn't match any overload at position 0
      // Even though second param is variable, we can detect this will definitely fail
      var ex = Assert.Throws<ParserException>(() =>
        compiler.Compile("calc(2.0, x)", Compiler.Options.Immutable, variables));

      Assert.Contains("calc", ex.Message);
    }

    [Fact]
    public void FunctionCall_WithVariableExpression_DefersToRuntime()
    {
      var compiler = CreateCompiler();
      compiler.AddCustomFunction(CustomFunction.Create("process", (int x) => x * 2));

      var variables = Variables().WithInteger("x", 5).Build();

      // x + 1 cannot be validated at compile time (x is variable)
      var script = compiler.Compile("process(x + 1)", Compiler.Options.Immutable, variables);

      // Should execute successfully
      script.Execute();
      Assert.Equal(12, script.IntegerValue); // (5+1)*2 = 12
    }

    #endregion

    #region Partial Validation with Variables - Parameter Count

    [Fact]
    public void FunctionCall_WrongParameterCount_AllVariables_ThrowsAtCompileTime()
    {
      var compiler = CreateCompiler();
      compiler.AddCustomFunction(CustomFunction.Create("getValue", (string s, float f) => true));
      compiler.AddCustomFunction(CustomFunction.Create("getValue", (bool b, float f) => false));

      var variables = Variables().WithInteger("x", 5).Build();

      // User's example: call has 1 param, all overloads need 2
      var ex = Assert.Throws<ParserException>(() =>
        compiler.Compile("getValue(x)", Compiler.Options.Immutable, variables));

      Assert.Contains("getValue", ex.Message);
    }

    [Fact]
    public void FunctionCall_WrongParameterCount_TooMany_ThrowsAtCompileTime()
    {
      var compiler = CreateCompiler();
      compiler.AddCustomFunction(CustomFunction.Create("double", (int x) => x * 2));

      var variables = Variables().WithInteger("x", 5).WithInteger("y", 3).Build();

      var ex = Assert.Throws<ParserException>(() =>
        compiler.Compile("double(x, y)", Compiler.Options.Immutable, variables));

      Assert.Contains("double", ex.Message);
    }

    #endregion

    #region Partial Validation with Variables - Single Concrete Parameter

    [Fact]
    public void FunctionCall_FirstParamConcreteMismatch_WithVariable_ThrowsAtCompileTime()
    {
      var compiler = CreateCompiler();
      compiler.AddCustomFunction(CustomFunction.Create("getValue", (string s, float f) => true));
      compiler.AddCustomFunction(CustomFunction.Create("getValue", (bool b, float f) => false));

      var variables = Variables().WithInteger("x", 5).Build();

      // User's example: 2.0 is float, but no overload accepts (float, *)
      var ex = Assert.Throws<ParserException>(() =>
        compiler.Compile("getValue(2.0, x)", Compiler.Options.Immutable, variables));

      Assert.Contains("getValue", ex.Message);
    }

    [Fact]
    public void FunctionCall_LastParamConcreteMismatch_WithVariable_ThrowsAtCompileTime()
    {
      var compiler = CreateCompiler();
      compiler.AddCustomFunction(CustomFunction.Create("test", (int i, float f) => i));
      compiler.AddCustomFunction(CustomFunction.Create("test", (int i, int i2) => i));

      var variables = Variables().WithInteger("x", 5).Build();

      // Last param is string, but all overloads need float/int
      var ex = Assert.Throws<ParserException>(() =>
        compiler.Compile("test(x, 'hello')", Compiler.Options.Immutable, variables));

      Assert.Contains("test", ex.Message);
    }

    [Fact]
    public void FunctionCall_FirstParamConcreteMatch_WithVariable_PassesValidation()
    {
      var compiler = CreateCompiler();
      compiler.AddCustomFunction(CustomFunction.Create("test", (string s, int i) => i));
      compiler.AddCustomFunction(CustomFunction.Create("test", (bool b, int i) => i));

      var variables = Variables().WithInteger("x", 5).Build();

      // First param is string, matches first overload at position 0
      // Second param is variable - defer to runtime
      var script = compiler.Compile("test('hello', x)", Compiler.Options.Immutable, variables);

      script.Execute();
      Assert.Equal(5, script.IntegerValue);
    }

    #endregion

    #region Partial Validation with Variables - Multiple Concrete Parameters

    [Fact]
    public void FunctionCall_MultipleConcreteParams_NoCommonOverload_ThrowsAtCompileTime()
    {
      var compiler = CreateCompiler();
      compiler.AddCustomFunction(CustomFunction.Create("test", (float f, int i, float f2) => f));
      compiler.AddCustomFunction(CustomFunction.Create("test", (float f, int i, int i2) => f));
      compiler.AddCustomFunction(CustomFunction.Create("test", (string s, int i, string s2) => 0f));

      var variables = Variables().WithInteger("x", 5).Build();

      // Position 0: float (matches overload 1,2), Position 2: string (matches overload 3)
      // No single overload satisfies both constraints
      var ex = Assert.Throws<ParserException>(() =>
        compiler.Compile("test(2.0, x, 'hello')", Compiler.Options.Immutable, variables));

      Assert.Contains("test", ex.Message);
    }

    [Fact]
    public void FunctionCall_MultipleConcreteParams_CommonOverloadExists_PassesValidation()
    {
      var compiler = CreateCompiler();
      compiler.AddCustomFunction(CustomFunction.Create("test", (float f, int i, float f2) => f));
      compiler.AddCustomFunction(CustomFunction.Create("test", (string s, int i, string s2) => 0f));

      var variables = Variables().WithInteger("x", 5).Build();

      // Position 0: float, Position 2: float - first overload matches both
      var script = compiler.Compile("test(2.0, x, 3.0)", Compiler.Options.Immutable, variables);

      script.Execute();
      Assert.Equal(2.0f, script.FloatValue);
    }

    #endregion

    #region Partial Validation - Type Conversion with Variables

    [Fact]
    public void FunctionCall_IntegerLiteralToFloat_WithVariable_PassesValidation()
    {
      var compiler = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      compiler.AddCustomFunction(CustomFunction.Create("test", (float f, int i) => f));

      var variables = Variables().WithInteger("x", 5).Build();

      // Integer literal 2 should convert to float
      var script = compiler.Compile("test(2, x)", Compiler.Options.Immutable, variables);

      script.Execute();
      Assert.Equal(2.0f, script.FloatValue);
    }

    [Fact]
    public void FunctionCall_IntegerConversionWrongPrecision_WithVariable_ThrowsAtCompileTime()
    {
      var compiler = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double);
      compiler.AddCustomFunction(CustomFunction.Create("test", (double d, int i) => d));

      var variables = Variables().WithInteger("x", 5).Build();

      // Integer→double conversion should work with Double precision, but string doesn't match
      var ex = Assert.Throws<ParserException>(() =>
        compiler.Compile("test('hello', x)", Compiler.Options.Immutable, variables));

      Assert.Contains("test", ex.Message);
    }

    #endregion

    #region Error Message Quality

    [Fact]
    public void FunctionValidation_ShowsAvailableOverloads()
    {
      var compiler = CreateCompiler();
      compiler.AddCustomFunction(CustomFunction.Create("calc", (int x) => x * 2));
      compiler.AddCustomFunction(CustomFunction.Create("calc", (string s) => s.Length));

      var ex = Assert.Throws<ParserException>(() => compiler.Compile("calc(2.0)"));

      // Should show both available overloads
      Assert.Contains("Available overloads:", ex.Message);
      Assert.Contains("calc(int)", ex.Message);
      Assert.Contains("calc(string)", ex.Message);
    }

    #endregion
  }
}