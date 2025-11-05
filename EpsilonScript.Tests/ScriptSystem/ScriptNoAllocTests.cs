using System;
using Xunit;

namespace EpsilonScript.Tests.ScriptSystem
{
  [Trait("Category", "Integration")]
  [Trait("Component", "ScriptSystem")]
  public class ScriptNoAllocTests : ScriptTestBase
  {
    #region Compile-Time Optimizations (Allowed)

    [Fact]
    public void NoAlloc_ConstantStringConcat_OptimizedAndAllowed()
    {
      // "BUILD_FLAG_" + 4 is optimized to "BUILD_FLAG_4" at compile time
      var compiler = CreateCompiler();
      var script = compiler.Compile("\"BUILD_FLAG_\" + 4", Compiler.Options.NoAlloc);

      // Should NOT throw - precomputable expression optimized away
      script.Execute();
      Assert.Equal("BUILD_FLAG_4", script.StringValue);
      Assert.True(script.IsPrecomputable, "Should be optimized to constant");
    }

    [Fact]
    public void NoAlloc_TwoStringLiterals_OptimizedAndAllowed()
    {
      // "hello" + "world" is precomputable, optimized to "helloworld"
      var compiler = CreateCompiler();
      var script = compiler.Compile("\"hello\" + \"world\"", Compiler.Options.NoAlloc);

      script.Execute();
      Assert.Equal("helloworld", script.StringValue);
    }

    [Fact]
    public void NoAlloc_ComplexConstantExpression_Optimized()
    {
      // "Result: " + (2 * 3) - tests MultiplyAddNode optimization
      var compiler = CreateCompiler();
      var script = compiler.Compile("\"Result: \" + (2 * 3)", Compiler.Options.NoAlloc);

      script.Execute();
      Assert.Equal("Result: 6", script.StringValue);
    }

    [Fact]
    public void NoAlloc_NestedConstantConcat_Optimized()
    {
      var compiler = CreateCompiler();
      var script = compiler.Compile("(\"A\" + \"B\") + (\"C\" + \"D\")", Compiler.Options.NoAlloc);

      script.Execute();
      Assert.Equal("ABCD", script.StringValue);
    }

    #endregion

    #region Runtime Allocations (Blocked)

    [Fact]
    public void NoAlloc_StringVariableConcat_Throws()
    {
      var variables = Variables().WithString("str", "hello").Build();
      var compiler = CreateCompiler();
      var script = compiler.Compile("str + \" world\"", Compiler.Options.NoAlloc, variables);

      var ex = Assert.Throws<RuntimeException>(() => script.Execute());
      Assert.Contains("String concatenation", ex.Message);
      Assert.Contains("NoAlloc mode", ex.Message);
    }

    [Fact]
    public void NoAlloc_TwoVariables_Throws()
    {
      var variables = Variables()
        .WithString("s1", "hello")
        .WithString("s2", "world")
        .Build();
      var compiler = CreateCompiler();
      var script = compiler.Compile("s1 + s2", Compiler.Options.NoAlloc, variables);

      var ex = Assert.Throws<RuntimeException>(() => script.Execute());
      Assert.Contains("String concatenation", ex.Message);
    }

    [Fact]
    public void NoAlloc_LiteralPlusVariable_Throws()
    {
      var variables = Variables().WithString("suffix", "_END").Build();
      var compiler = CreateCompiler();
      var script = compiler.Compile("\"PREFIX\" + suffix", Compiler.Options.NoAlloc, variables);

      var ex = Assert.Throws<RuntimeException>(() => script.Execute());
      Assert.Contains("String concatenation", ex.Message);
    }

    [Fact]
    public void NoAlloc_VariablePlusNumericMultiply_Throws()
    {
      var variables = Variables().WithString("prefix", "Count: ").Build();
      var compiler = CreateCompiler();
      var script = compiler.Compile("prefix + (10 * 5)", Compiler.Options.NoAlloc, variables);

      var ex = Assert.Throws<RuntimeException>(() => script.Execute());
      Assert.Contains("String concatenation", ex.Message);
    }

    #endregion

    #region Allowed Operations (No Allocation)

    [Fact]
    public void NoAlloc_NumericOperations_Allowed()
    {
      var compiler = CreateCompiler();
      var script = compiler.Compile("10 + 20 * 3", Compiler.Options.NoAlloc);
      script.Execute();

      Assert.Equal(70, script.IntegerValue);
    }

    [Fact]
    public void NoAlloc_StringLiteral_Allowed()
    {
      var compiler = CreateCompiler();
      var script = compiler.Compile("\"hello\"", Compiler.Options.NoAlloc);
      script.Execute();

      Assert.Equal("hello", script.StringValue);
    }

    [Fact]
    public void NoAlloc_StringVariable_Allowed()
    {
      // Reading string variable is just a reference (no allocation)
      var variables = Variables().WithString("myString", "test").Build();
      var compiler = CreateCompiler();
      var script = compiler.Compile("myString", Compiler.Options.NoAlloc, variables);
      script.Execute();

      Assert.Equal("test", script.StringValue);
    }

    [Fact]
    public void NoAlloc_NumericVariables_Allowed()
    {
      var variables = Variables()
        .WithInteger("a", 10)
        .WithInteger("b", 20)
        .Build();
      var compiler = CreateCompiler();
      var script = compiler.Compile("a + b * 2", Compiler.Options.NoAlloc, variables);
      script.Execute();

      Assert.Equal(50, script.IntegerValue);
    }

    [Fact]
    public void NoAlloc_BooleanOperations_Allowed()
    {
      var compiler = CreateCompiler();
      var script = compiler.Compile("true && (5 > 3)", Compiler.Options.NoAlloc);
      script.Execute();

      Assert.True(script.BooleanValue);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void NoAlloc_DynamicTypePromotion_Throws()
    {
      // Even if variables are numeric, string literal promotes to string
      var variables = Variables().WithInteger("num", 42).Build();
      var compiler = CreateCompiler();
      var script = compiler.Compile("\"Value: \" + num", Compiler.Options.NoAlloc, variables);

      var ex = Assert.Throws<RuntimeException>(() => script.Execute());
      Assert.Contains("String concatenation", ex.Message);
    }

    [Fact]
    public void NoAlloc_WithImmutable_BothEnforced()
    {
      var variables = Variables().WithInteger("x", 0).Build();
      var compiler = CreateCompiler();

      // Test Immutable enforcement
      var ex1 = Assert.Throws<ParserException>(() =>
        compiler.Compile("x = 5", Compiler.Options.NoAlloc | Compiler.Options.Immutable, variables));
      Assert.Contains("immutable", ex1.Message);

      // Test NoAlloc allows optimized constants
      var script2 = compiler.Compile("\"a\" + \"b\"", Compiler.Options.NoAlloc | Compiler.Options.Immutable);
      script2.Execute();
      Assert.Equal("ab", script2.StringValue);
    }

    [Fact]
    public void NoAlloc_MultipleExecutions_ConsistentBehavior()
    {
      // Verify optimized constants can be executed multiple times
      var compiler = CreateCompiler();
      var script = compiler.Compile("\"X\" + \"Y\"", Compiler.Options.NoAlloc);

      // Multiple executions should all work (optimized to constant)
      for (var i = 0; i < 100; i++)
      {
        script.Execute();
        Assert.Equal("XY", script.StringValue);
      }
    }

    [Fact]
    public void NoAlloc_PrecomputableCheck_VerifyIsPrecomputable()
    {
      // Verify that constant expressions are actually marked as precomputable
      var compiler = CreateCompiler();

      var script1 = compiler.Compile("\"a\" + \"b\"", Compiler.Options.NoAlloc);
      Assert.True(script1.IsPrecomputable, "Constant string concat should be precomputable");

      var variables = Variables().WithString("v", "x").Build();
      var script2 = compiler.Compile("v + \"b\"", Compiler.Options.NoAlloc, variables);
      Assert.False(script2.IsPrecomputable, "Variable concat should not be precomputable");
    }

    [Fact]
    public void NoAlloc_SequenceWithStringConcat_Throws()
    {
      // Test that string concatenation is blocked in sequence expressions
      var variables = Variables().WithString("s", "test").Build();
      var compiler = CreateCompiler();
      var script = compiler.Compile("1 + 1; s + \"_suffix\"", Compiler.Options.NoAlloc, variables);

      var ex = Assert.Throws<RuntimeException>(() => script.Execute());
      Assert.Contains("String concatenation", ex.Message);
    }

    [Fact]
    public void NoAlloc_ConstantInSequence_Allowed()
    {
      // Constant string concat in sequence should be optimized and allowed
      var compiler = CreateCompiler();
      var script = compiler.Compile("1 + 1; \"A\" + \"B\"", Compiler.Options.NoAlloc);

      script.Execute();
      Assert.Equal("AB", script.StringValue);
    }

    [Fact]
    public void NoAlloc_StringConcatWithFloatResult_Throws()
    {
      var variables = Variables().WithString("prefix", "PI: ").Build();
      var compiler = CreateCompiler();
      var script = compiler.Compile("prefix + 3.14", Compiler.Options.NoAlloc, variables);

      var ex = Assert.Throws<RuntimeException>(() => script.Execute());
      Assert.Contains("String concatenation", ex.Message);
    }

    [Fact]
    public void NoAlloc_ConstantFloatStringConcat_OptimizedAndAllowed()
    {
      var compiler = CreateCompiler();
      var script = compiler.Compile("\"PI: \" + 3.14", Compiler.Options.NoAlloc);

      script.Execute();
      Assert.Equal("PI: 3.14", script.StringValue);
    }

    #endregion

    #region Variable Assignment Allocations

    [Fact]
    public void NoAlloc_AssignIntegerToStringVariable_Throws()
    {
      // Assigning integer to string variable calls ToString() which allocates
      var variables = Variables().WithString("str", "initial").Build();
      var compiler = CreateCompiler();
      var script = compiler.Compile("str = 42", Compiler.Options.NoAlloc, variables);

      var ex = Assert.Throws<RuntimeException>(() => script.Execute());
      Assert.Contains("Assigning non-string value to string variable", ex.Message);
      Assert.Contains("ToString()", ex.Message);
      Assert.Contains("NoAlloc mode", ex.Message);
    }

    [Fact]
    public void NoAlloc_AssignFloatToStringVariable_Throws()
    {
      var variables = Variables().WithString("str", "initial").Build();
      var compiler = CreateCompiler();
      var script = compiler.Compile("str = 3.14", Compiler.Options.NoAlloc, variables);

      var ex = Assert.Throws<RuntimeException>(() => script.Execute());
      Assert.Contains("Assigning non-string value to string variable", ex.Message);
    }

    [Fact]
    public void NoAlloc_AssignBooleanToStringVariable_Throws()
    {
      var variables = Variables().WithString("str", "initial").Build();
      var compiler = CreateCompiler();
      var script = compiler.Compile("str = true", Compiler.Options.NoAlloc, variables);

      var ex = Assert.Throws<RuntimeException>(() => script.Execute());
      Assert.Contains("Assigning non-string value to string variable", ex.Message);
    }

    [Fact]
    public void NoAlloc_AssignNumericExpressionToStringVariable_Throws()
    {
      var variables = Variables().WithString("str", "initial").Build();
      var compiler = CreateCompiler();
      var script = compiler.Compile("str = 10 + 20", Compiler.Options.NoAlloc, variables);

      var ex = Assert.Throws<RuntimeException>(() => script.Execute());
      Assert.Contains("Assigning non-string value to string variable", ex.Message);
    }

    [Fact]
    public void NoAlloc_AssignStringToStringVariable_Allowed()
    {
      // Assigning string to string variable does not allocate (just reference copy)
      var variables = Variables().WithString("str", "initial").Build();
      var compiler = CreateCompiler();
      var script = compiler.Compile("str = \"updated\"", Compiler.Options.NoAlloc, variables);

      script.Execute();
      Assert.Equal("updated", variables["str"].StringValue);
    }

    [Fact]
    public void NoAlloc_AssignStringVariableToStringVariable_Allowed()
    {
      var variables = Variables()
        .WithString("str1", "source")
        .WithString("str2", "target")
        .Build();
      var compiler = CreateCompiler();
      var script = compiler.Compile("str2 = str1", Compiler.Options.NoAlloc, variables);

      script.Execute();
      Assert.Equal("source", variables["str2"].StringValue);
    }

    [Fact]
    public void NoAlloc_AssignIntegerToIntegerVariable_Allowed()
    {
      // Non-string assignments don't allocate
      var variables = Variables().WithInteger("num", 10).Build();
      var compiler = CreateCompiler();
      var script = compiler.Compile("num = 42", Compiler.Options.NoAlloc, variables);

      script.Execute();
      Assert.Equal(42, variables["num"].IntegerValue);
    }

    [Fact]
    public void NoAlloc_AssignExpressionToIntegerVariable_Allowed()
    {
      var variables = Variables().WithInteger("num", 0).Build();
      var compiler = CreateCompiler();
      var script = compiler.Compile("num = 10 + 20 * 3", Compiler.Options.NoAlloc, variables);

      script.Execute();
      Assert.Equal(70, variables["num"].IntegerValue);
    }

    #endregion
  }
}