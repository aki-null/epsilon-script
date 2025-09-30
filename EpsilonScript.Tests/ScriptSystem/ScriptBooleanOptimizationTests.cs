using Xunit;
using EpsilonScript.Function;

namespace EpsilonScript.Tests.ScriptSystem
{
  [Trait("Category", "Integration")]
  [Trait("Component", "ScriptSystem")]
  public class ScriptBooleanOptimizationTests : ScriptTestBase
  {
    [Fact]
    public void BooleanAnd_FalseAndExpression_OptimizesToFalse()
    {
      // Test that "false && expensive_call()" doesn't call expensive_call
      var (function, counter) = TestFunctions.CreateBooleanProbe("expensive_call");
      var compiler = CreateCompiler(function);

      var script = compiler.Compile("false && expensive_call(true)", Compiler.Options.Immutable);
      script.Execute();

      Assert.Equal(Type.Boolean, script.ValueType);
      Assert.False(script.BooleanValue);
      // The function should not have been called due to short-circuit optimization
      Assert.Equal(0, counter.Count);
    }

    [Fact]
    public void BooleanOr_TrueOrExpression_OptimizesToTrue()
    {
      // Test that "true || expensive_call()" doesn't call expensive_call
      var (function, counter) = TestFunctions.CreateBooleanProbe("expensive_call");
      var compiler = CreateCompiler(function);

      var script = compiler.Compile("true || expensive_call(false)", Compiler.Options.Immutable);
      script.Execute();

      Assert.Equal(Type.Boolean, script.ValueType);
      Assert.True(script.BooleanValue);
      // The function should not have been called due to short-circuit optimization
      Assert.Equal(0, counter.Count);
    }

    [Fact]
    public void BooleanAnd_TrueAndExpression_OptimizesToExpression()
    {
      // Test that "true && expression" optimizes to just "expression"
      var (function, counter) = TestFunctions.CreateBooleanProbe("test_call");
      var compiler = CreateCompiler(function);

      var script = compiler.Compile("true && test_call(false)", Compiler.Options.Immutable);
      script.Execute();

      Assert.Equal(Type.Boolean, script.ValueType);
      Assert.False(script.BooleanValue);
      // The function should have been called once (optimization kept the expression)
      Assert.Equal(1, counter.Count);
    }

    [Fact]
    public void BooleanOr_FalseOrExpression_OptimizesToExpression()
    {
      // Test that "false || expression" optimizes to just "expression"
      var (function, counter) = TestFunctions.CreateBooleanProbe("test_call");
      var compiler = CreateCompiler(function);

      var script = compiler.Compile("false || test_call(true)", Compiler.Options.Immutable);
      script.Execute();

      Assert.Equal(Type.Boolean, script.ValueType);
      Assert.True(script.BooleanValue);
      // The function should have been called once (optimization kept the expression)
      Assert.Equal(1, counter.Count);
    }

    [Fact]
    public void BooleanAnd_NonConstantFunctionAndFalse_CompileTimeOptimization()
    {
      // Test that "non_constant_function() && false" is optimized at compile-time to false
      var (function, counter) = TestFunctions.CreateBooleanProbe("expensive_call", isConstant: false);
      var compiler = CreateCompiler(function);

      var script = compiler.Compile("expensive_call(true) && false", Compiler.Options.Immutable);
      script.Execute();

      Assert.Equal(Type.Boolean, script.ValueType);
      Assert.False(script.BooleanValue);
      // Compile-time short-circuit optimization eliminates function call completely
      Assert.Equal(0, counter.Count);
    }

    [Fact]
    public void BooleanOr_NonConstantFunctionOrTrue_CompileTimeOptimization()
    {
      // Test that "non_constant_function() || true" is optimized to true at compile time
      // Since all EpsilonScript functions are pure, even non-constant functions can be optimized away
      var (function, counter) = TestFunctions.CreateBooleanProbe("expensive_call", isConstant: false);
      var compiler = CreateCompiler(function);

      var script = compiler.Compile("expensive_call(false) || true", Compiler.Options.Immutable);
      script.Execute();

      Assert.Equal(Type.Boolean, script.ValueType);
      Assert.True(script.BooleanValue);
      // Compile-time short-circuit optimization eliminates function call completely
      Assert.Equal(0, counter.Count);
    }

    [Fact]
    public void BooleanAnd_ExpressionAndTrue_OptimizesToExpression()
    {
      // Test that "expression && true" optimizes to just "expression"
      var (function, counter) = TestFunctions.CreateBooleanProbe("test_call");
      var compiler = CreateCompiler(function);

      var script = compiler.Compile("test_call(false) && true", Compiler.Options.Immutable);
      script.Execute();

      Assert.Equal(Type.Boolean, script.ValueType);
      Assert.False(script.BooleanValue);
      // The function should have been called once (optimization kept the expression)
      Assert.Equal(1, counter.Count);
    }

    [Fact]
    public void BooleanOr_ExpressionOrFalse_OptimizesToExpression()
    {
      // Test that "expression || false" optimizes to just "expression"
      var (function, counter) = TestFunctions.CreateBooleanProbe("test_call");
      var compiler = CreateCompiler(function);

      var script = compiler.Compile("test_call(true) || false", Compiler.Options.Immutable);
      script.Execute();

      Assert.Equal(Type.Boolean, script.ValueType);
      Assert.True(script.BooleanValue);
      // The function should have been called once (optimization kept the expression)
      Assert.Equal(1, counter.Count);
    }

    [Fact]
    public void ComplexBooleanExpression_WithOptimizations()
    {
      // Test a more complex expression: false && (expensive() || true)
      // This should optimize to just false without calling expensive()
      var (function, counter) = TestFunctions.CreateBooleanProbe("expensive");
      var compiler = CreateCompiler(function);

      var script = compiler.Compile("false && (expensive(true) || true)", Compiler.Options.Immutable);
      script.Execute();

      Assert.Equal(Type.Boolean, script.ValueType);
      Assert.False(script.BooleanValue);
      // No function calls should occur due to optimization
      Assert.Equal(0, counter.Count);
    }

    [Fact]
    public void NestedBooleanExpression_WithPartialOptimizations()
    {
      // Test: (true && call1()) || (false && call2())
      // Should optimize to: call1() || false => call1()
      var (function1, counter1) = TestFunctions.CreateBooleanProbe("call1");
      var (function2, counter2) = TestFunctions.CreateBooleanProbe("call2");
      var compiler = CreateCompiler(function1, function2);

      var script = compiler.Compile("(true && call1(true)) || (false && call2(false))", Compiler.Options.Immutable);
      script.Execute();

      Assert.Equal(Type.Boolean, script.ValueType);
      Assert.True(script.BooleanValue);
      // Only call1 should be executed, call2 should be optimized away
      Assert.Equal(1, counter1.Count);
      Assert.Equal(0, counter2.Count);
    }

    [Fact]
    public void BooleanAnd_ConstantAndFalse_OptimizesToFalse()
    {
      // Test that "constant && false" optimizes to just false (no side effects)
      var (function, counter) = TestFunctions.CreateBooleanProbe("should_not_be_called");
      var compiler = CreateCompiler(function);

      var script = compiler.Compile("(5 > 3) && false", Compiler.Options.Immutable);
      script.Execute();

      Assert.Equal(Type.Boolean, script.ValueType);
      Assert.False(script.BooleanValue);
      // No function should be called
      Assert.Equal(0, counter.Count);
    }

    [Fact]
    public void BooleanOr_ConstantOrTrue_OptimizesToTrue()
    {
      // Test that "constant || true" optimizes to just true (no side effects)
      var (function, counter) = TestFunctions.CreateBooleanProbe("should_not_be_called");
      var compiler = CreateCompiler(function);

      var script = compiler.Compile("(5 < 3) || true", Compiler.Options.Immutable);
      script.Execute();

      Assert.Equal(Type.Boolean, script.ValueType);
      Assert.True(script.BooleanValue);
      // No function should be called
      Assert.Equal(0, counter.Count);
    }

    [Fact]
    public void BooleanAnd_ConstantFunctionAndFalse_OptimizedAway()
    {
      // Test that "constant_function() && false" is optimized to false without evaluating function
      var (function, counter) = TestFunctions.CreateBooleanProbe("constant_func", isConstant: true);
      var compiler = CreateCompiler(function);

      var script = compiler.Compile("constant_func(true) && false", Compiler.Options.Immutable);
      script.Execute();

      Assert.Equal(Type.Boolean, script.ValueType);
      Assert.False(script.BooleanValue);
      // Short-circuit optimization should eliminate function call completely
      Assert.Equal(0, counter.Count);
    }

    [Fact]
    public void BooleanOr_ConstantFunctionOrTrue_OptimizedAway()
    {
      // Test that "constant_function() || true" is optimized to true without evaluating function
      var (function, counter) = TestFunctions.CreateBooleanProbe("constant_func", isConstant: true);
      var compiler = CreateCompiler(function);

      var script = compiler.Compile("constant_func(false) || true", Compiler.Options.Immutable);
      script.Execute();

      Assert.Equal(Type.Boolean, script.ValueType);
      Assert.True(script.BooleanValue);
      // Short-circuit optimization should eliminate function call completely
      Assert.Equal(0, counter.Count);
    }
  }
}