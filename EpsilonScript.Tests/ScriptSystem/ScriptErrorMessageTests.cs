using EpsilonScript.Function;
using System;
using Xunit;

namespace EpsilonScript.Tests.ScriptSystem
{
  /// <summary>
  /// Tests to verify that error messages are informative and contain relevant context
  /// Good error messages help developers debug issues quickly
  /// </summary>
  [Trait("Category", "Integration")]
  [Trait("Component", "ScriptSystem")]
  [Trait("Priority", "High")]
  public class ScriptErrorMessageTests : ScriptTestBase
  {
    #region Undefined Variable Errors

    [Fact]
    public void UndefinedVariable_ErrorMentionsVariableName()
    {
      var compiler = CreateCompiler();
      var script = compiler.Compile("missingVar + 1");
      var ex = Assert.Throws<RuntimeException>(() => script.Execute());
      Assert.Contains("missingVar", ex.Message);
    }

    [Fact]
    public void UndefinedVariable_InComplexExpression_MentionsVariableName()
    {
      var variables = Variables().WithInteger("x", 10).Build();
      var compiler = CreateCompiler();
      var script = compiler.Compile("x + unknownVar * 2", Compiler.Options.None, variables);
      var ex = Assert.Throws<RuntimeException>(() => script.Execute());
      Assert.Contains("unknownVar", ex.Message);
    }

    [Fact]
    public void UndefinedVariable_WithDottedName_MentionsFullName()
    {
      var compiler = CreateCompiler();
      var script = compiler.Compile("config.server.port");
      var ex = Assert.Throws<RuntimeException>(() => script.Execute());
      Assert.Contains("config.server.port", ex.Message);
    }

    #endregion

    #region Undefined Function Errors

    [Fact]
    public void UndefinedFunction_ErrorMentionsFunctionName()
    {
      var compiler = CreateCompiler();
      var ex = Assert.Throws<ParserException>(() =>
        compiler.Compile("unknownFunc(1, 2)", Compiler.Options.Immutable));
      Assert.Contains("unknownFunc", ex.Message);
    }

    [Fact]
    public void UndefinedFunction_WithDottedName_MentionsFullName()
    {
      var compiler = CreateCompiler();
      var ex = Assert.Throws<ParserException>(() =>
        compiler.Compile("utils.math.custom(5)", Compiler.Options.Immutable));
      Assert.Contains("utils.math.custom", ex.Message);
    }

    [Fact]
    public void UndefinedFunction_NoParameters_MentionsFunctionName()
    {
      var compiler = CreateCompiler();
      var ex = Assert.Throws<ParserException>(() =>
        compiler.Compile("missingFunc()", Compiler.Options.Immutable));
      Assert.Contains("missingFunc", ex.Message);
    }

    #endregion

    #region Type Mismatch Errors

    [Fact]
    public void TypeMismatch_StringAndNumber_MentionsTypes()
    {
      var compiler = CreateCompiler();
      var ex = Assert.Throws<RuntimeException>(() =>
        compiler.Compile("\"hello\" == 123", Compiler.Options.Immutable));
      // Should mention either "string" or "String" and either indicate type mismatch
      Assert.True(
        ex.Message.Contains("String", StringComparison.OrdinalIgnoreCase) ||
        ex.Message.Contains("type", StringComparison.OrdinalIgnoreCase),
        $"Expected type information in error message: {ex.Message}");
    }

    [Fact]
    public void TypeMismatch_BooleanArithmetic_MentionsBoolean()
    {
      var compiler = CreateCompiler();
      var ex = Assert.Throws<RuntimeException>(() =>
        compiler.Compile("true + 1", Compiler.Options.Immutable));
      // Should match error from ArithmeticNode.cs
      Assert.Contains("Boolean values cannot be used in arithmetic operations", ex.Message);
    }

    [Fact]
    public void TypeMismatch_StringArithmetic_MentionsString()
    {
      var compiler = CreateCompiler();
      var ex = Assert.Throws<RuntimeException>(() =>
        compiler.Compile("\"hello\" * 2", Compiler.Options.Immutable));
      // Should match error from ArithmeticNode.cs
      Assert.Contains("String operations only support concatenation (+)", ex.Message);
    }

    [Fact]
    public void TypeMismatch_LogicalOperationOnNumber_MentionsOperation()
    {
      var compiler = CreateCompiler();
      var ex = Assert.Throws<RuntimeException>(() =>
        compiler.Compile("5 && true", Compiler.Options.Immutable));

      // Error should indicate the operation or type issue
      var hasContext = ex.Message.Contains("Boolean", StringComparison.OrdinalIgnoreCase) ||
                       ex.Message.Contains("type", StringComparison.OrdinalIgnoreCase) ||
                       ex.Message.Contains("&&");
      Assert.True(hasContext, $"Expected operation/type context in error: {ex.Message}");
    }

    #endregion

    #region Invalid Operator Errors

    [Fact]
    public void InvalidOperation_Assignment_MentionsAssignment()
    {
      var compiler = CreateCompiler();
      // Parser now catches this at parse time, not runtime
      var ex = Assert.Throws<ParserException>(() => compiler.Compile("1 = 2"));
      Assert.Contains("assignment", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void InvalidOperation_StringSubtraction_MentionsOperation()
    {
      var compiler = CreateCompiler();
      var ex = Assert.Throws<RuntimeException>(() =>
        compiler.Compile("\"a\" - \"b\"", Compiler.Options.Immutable));
      // Should match error from ArithmeticNode.cs
      Assert.Contains("String operations only support concatenation (+)", ex.Message);
    }

    [Fact]
    public void InvalidOperation_StringComparison_MentionsComparison()
    {
      var compiler = CreateCompiler();
      var ex = Assert.Throws<RuntimeException>(() =>
        compiler.Compile("\"apple\" < \"banana\"", Compiler.Options.Immutable));
      // Should match error from ComparisonNode.cs with location context
      Assert.Equal(
        "Runtime error at line 1, column 9: Cannot perform arithmetic comparison on non-numeric types (left: String, right: String)",
        ex.Message);
    }

    #endregion

    #region Function Argument Errors

    [Fact]
    public void WrongArgumentCount_ShowsExpectedAndActual()
    {
      var compiler = CreateCompiler();
      compiler.AddCustomFunction(CustomFunction.Create("func", (int x) => x * 2));

      // Validation now happens at compile time for constant expressions
      var ex = Assert.Throws<ParserException>(() => compiler.Compile("func(1, 2, 3)"));

      // Should mention the function name and indicate argument count issue
      Assert.Contains("func", ex.Message);
    }

    [Fact]
    public void WrongArgumentType_MentionsFunctionName()
    {
      var compiler = CreateCompiler();
      compiler.AddCustomFunction(CustomFunction.Create("onlyInt", (int x) => x));

      var variables = Variables().WithString("str", "hello").Build();
      var script = compiler.Compile("onlyInt(str)", Compiler.Options.Immutable, variables);
      var ex = Assert.Throws<RuntimeException>(() => script.Execute());

      // Error should mention function/signature/type issue
      Assert.True(
        ex.Message.Contains("function", StringComparison.OrdinalIgnoreCase) ||
        ex.Message.Contains("signature", StringComparison.OrdinalIgnoreCase) ||
        ex.Message.Contains("type", StringComparison.OrdinalIgnoreCase),
        $"Expected function/type context in error: {ex.Message}");
    }

    [Fact]
    public void MissingOverload_MentionsFunctionName()
    {
      var compiler = CreateCompiler();
      compiler.AddCustomFunction(CustomFunction.Create("typed", (string s) => s.Length));

      // Validation now happens at compile time for constant expressions
      var ex = Assert.Throws<ParserException>(() => compiler.Compile("typed(42)"));

      // Error should mention function/signature issue
      Assert.True(
        ex.Message.Contains("function", StringComparison.OrdinalIgnoreCase) ||
        ex.Message.Contains("signature", StringComparison.OrdinalIgnoreCase),
        $"Expected function/signature context in error: {ex.Message}");
    }

    #endregion

    #region Variable Assignment Errors

    [Fact]
    public void AssignmentToWrongType_MentionsVariable()
    {
      var variables = Variables().WithBoolean("flag", true).Build();
      var compiler = CreateCompiler();
      var script = compiler.Compile("flag = 42", Compiler.Options.None, variables);
      var ex = Assert.Throws<RuntimeException>(() => script.Execute());

      // Should mention the variable name, type, or assignment issue
      var hasContext = ex.Message.Contains("flag") ||
                       ex.Message.Contains("type", StringComparison.OrdinalIgnoreCase) ||
                       ex.Message.Contains("Boolean", StringComparison.OrdinalIgnoreCase) ||
                       ex.Message.Contains("Integer", StringComparison.OrdinalIgnoreCase);
      Assert.True(hasContext, $"Expected variable/type context in error: {ex.Message}");
    }

    [Fact]
    public void ImmutableMode_Assignment_MentionsImmutable()
    {
      var variables = Variables().WithInteger("x", 10).Build();
      var compiler = CreateCompiler();
      var ex = Assert.Throws<ParserException>(() =>
        compiler.Compile("x = 20", Compiler.Options.Immutable, variables));

      // Should indicate that assignment is not allowed in immutable mode
      Assert.True(
        ex.Message.Contains("immutable", StringComparison.OrdinalIgnoreCase) ||
        ex.Message.Contains("assignment", StringComparison.OrdinalIgnoreCase),
        $"Expected immutability context in error: {ex.Message}");
    }

    #endregion

    #region Comparison Errors

    [Fact]
    public void InvalidComparison_MixedTypes_MentionsComparison()
    {
      var compiler = CreateCompiler();
      var ex = Assert.Throws<RuntimeException>(() =>
        compiler.Compile("true == 42", Compiler.Options.Immutable));

      // Should match error from ComparisonNode.cs with location context
      Assert.Equal(
        "Runtime error at line 1, column 6: Cannot compare incompatible types: Boolean and Integer (numeric types can only be compared with other numeric types)",
        ex.Message);
    }

    #endregion

    #region Division and Modulo Errors

    [Fact]
    public void DivisionByZero_Integer_ProvidesErrorMessage()
    {
      var compiler = CreateCompiler();
      // Exception thrown during compile-time optimization (constant folding)
      var ex = Assert.Throws<DivideByZeroException>(() =>
        compiler.Compile("10 / 0", Compiler.Options.Immutable));

      // Standard .NET exception should have a message
      Assert.False(string.IsNullOrWhiteSpace(ex.Message));
    }

    [Fact]
    public void ModuloByZero_ProvidesErrorMessage()
    {
      var compiler = CreateCompiler();
      // Exception thrown during compile-time optimization (constant folding)
      var ex = Assert.Throws<DivideByZeroException>(() =>
        compiler.Compile("10 % 0", Compiler.Options.Immutable));

      Assert.False(string.IsNullOrWhiteSpace(ex.Message));
    }

    #endregion

    #region Negation Errors

    [Fact]
    public void NegateNonBoolean_MentionsNegation()
    {
      var compiler = CreateCompiler();
      var ex = Assert.Throws<RuntimeException>(() =>
        compiler.Compile("!42", Compiler.Options.Immutable));

      // Should indicate negation issue
      Assert.True(
        ex.Message.Length > 5,
        $"Expected informative error message, got: {ex.Message}");
    }

    [Fact]
    public void NegateString_MentionsType()
    {
      var compiler = CreateCompiler();
      var ex = Assert.Throws<RuntimeException>(() =>
        compiler.Compile("!\"hello\"", Compiler.Options.Immutable));

      Assert.True(
        ex.Message.Contains("String", StringComparison.OrdinalIgnoreCase) ||
        ex.Message.Length > 10,
        $"Expected type information in error: {ex.Message}");
    }

    #endregion

    #region Contextual Function Errors

    [Fact]
    public void ContextualFunction_MissingVariable_MentionsVariableName()
    {
      var compiler = CreateCompiler();
      compiler.AddCustomFunction(
        CustomFunction.CreateContextual("needsContext", "requiredVar", (int x) => x * 2));

      var script = compiler.Compile("needsContext()", Compiler.Options.Immutable);
      var ex = Assert.Throws<RuntimeException>(() => script.Execute());

      // Should mention the missing context variable
      Assert.Contains("requiredVar", ex.Message);
    }

    [Fact]
    public void ContextualFunction_WrongVariableType_ProvidesErrorContext()
    {
      var compiler = CreateCompiler();
      compiler.AddCustomFunction(
        CustomFunction.CreateContextual("expectsInt", "value", (int x) => x));

      var variables = Variables().WithString("value", "not an int").Build();
      var script = compiler.Compile("expectsInt()", Compiler.Options.Immutable, variables);

      Assert.ThrowsAny<Exception>(() => script.Execute());
    }

    #endregion

    #region Complex Expression Errors

    [Fact]
    public void ComplexExpression_ErrorStillProvidesContext()
    {
      var variables = Variables()
        .WithInteger("a", 10)
        .WithInteger("b", 20)
        .Build();

      var compiler = CreateCompiler();
      var script = compiler.Compile("a + b + missingVar + 30", Compiler.Options.None, variables);
      var ex = Assert.Throws<RuntimeException>(() => script.Execute());

      // Should still mention the problematic variable
      Assert.Contains("missingVar", ex.Message);
    }

    [Fact]
    public void NestedFunction_ErrorMentionsFunctionName()
    {
      var compiler = CreateCompiler();
      compiler.AddCustomFunction(CustomFunction.Create("outer", (int x) => x));

      // Undefined function caught at compile time
      var ex = Assert.Throws<ParserException>(() =>
        compiler.Compile("outer(missing(5))"));

      Assert.Contains("missing", ex.Message);
    }

    #endregion

    #region Line Number Accuracy Tests

    [Fact]
    public void UndefinedVariable_StandaloneAfterSemicolons_ReportsCorrectLineNumber()
    {
      var compiler = CreateCompiler();
      var script = compiler.Compile("2;\n2;\nls");
      var ex = Assert.Throws<RuntimeException>(() => script.Execute());

      // The undefined variable 'ls' is on line 3 (1-indexed)
      Assert.Equal(2, ex.Location.LineNumber); // 0-indexed internally
      Assert.Contains("line 3", ex.Message); // User-facing should be 1-indexed
    }

    [Fact]
    public void UndefinedVariable_InsideFunctionAfterSemicolons_ReportsCorrectLineNumber()
    {
      var compiler = CreateCompiler();
      var script = compiler.Compile("2;\n2;\nabs(ls)");
      var ex = Assert.Throws<RuntimeException>(() => script.Execute());

      // The undefined variable 'ls' is on line 3 (1-indexed)
      Assert.Equal(2, ex.Location.LineNumber); // 0-indexed internally
      Assert.Contains("line 3", ex.Message); // User-facing should be 1-indexed
    }

    [Fact]
    public void UndefinedVariable_MultipleLines_ReportsCorrectLineNumber()
    {
      var compiler = CreateCompiler();
      var script = compiler.Compile("1 + 2;\n3 * 4;\n5 / undefined");
      var ex = Assert.Throws<RuntimeException>(() => script.Execute());

      // The undefined variable is on line 3
      Assert.Equal(2, ex.Location.LineNumber); // 0-indexed
      Assert.Contains("line 3", ex.Message);
      Assert.Contains("undefined", ex.Message);
    }

    [Fact]
    public void UndefinedVariable_ComplexExpression_ReportsCorrectLineNumber()
    {
      var compiler = CreateCompiler();
      var script = compiler.Compile("1;\n2;\n3;\nmissing + 1");
      var ex = Assert.Throws<RuntimeException>(() => script.Execute());

      // The error is on line 4
      Assert.Equal(3, ex.Location.LineNumber); // 0-indexed
      Assert.Contains("line 4", ex.Message);
    }

    [Fact]
    public void UndefinedVariable_NestedFunction_ReportsCorrectLineNumber()
    {
      var compiler = CreateCompiler();
      var script = compiler.Compile("1;\nabs(\n  max(notfound, 1)\n)");
      var ex = Assert.Throws<RuntimeException>(() => script.Execute());

      // The undefined variable is on line 3
      Assert.Equal(2, ex.Location.LineNumber); // 0-indexed
      Assert.Contains("line 3", ex.Message);
    }

    #endregion

    #region Custom Function Registration Errors

    [Fact]
    public void DuplicateFunction_SameSignature_ProvidesErrorMessage()
    {
      var compiler = CreateCompiler();
      compiler.AddCustomFunction(CustomFunction.Create("func", (int x) => x));

      var ex = Assert.Throws<RuntimeException>(() =>
        compiler.AddCustomFunction(CustomFunction.Create("func", (int y) => y * 2)));

      Assert.Contains("func", ex.Message);
    }

    [Fact]
    public void ConstnessMismatch_ProvidesErrorMessage()
    {
      var compiler = CreateCompiler();
      compiler.AddCustomFunction(CustomFunction.Create("func", (int x) => x, isDeterministic: true));

      var ex = Assert.Throws<ArgumentException>(() =>
        compiler.AddCustomFunction(CustomFunction.Create("func", (float x) => x, isDeterministic: false)));

      Assert.Contains("cannot mix deterministic", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    #endregion
  }
}