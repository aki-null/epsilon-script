using EpsilonScript.Function;
using EpsilonScript.Tests.TestInfrastructure;
using Xunit;

namespace EpsilonScript.Tests.ScriptSystem
{
  /// <summary>
  /// Tests to verify compliance with LANGUAGE.md specification at the integration level
  /// These tests ensure forbidden operations are properly rejected
  /// </summary>
  [Trait("Category", "Integration")]
  [Trait("Component", "ScriptSystem")]
  [Trait("Priority", "Critical")]
  public class ScriptSpecComplianceTests : ScriptTestBase
  {
    #region String Operations (Per Spec: Only + is supported)

    /// <summary>
    /// Per LANGUAGE.md: "String with -, *, /, % → Runtime error (only + supported for strings)"
    /// </summary>
    [Theory]
    [InlineData("\"hello\" - \"world\"")]
    [InlineData("\"test\" * \"data\"")]
    [InlineData("\"numerator\" / \"denominator\"")]
    [InlineData("\"left\" % \"right\"")]
    public void String_ArithmeticOperatorsOtherThanPlus_ThrowsRuntimeException(string expression)
    {
      var compiler = CreateCompiler();
      ErrorTestHelper.AssertRuntimeException(
        () => compiler.Compile(expression, Compiler.Options.Immutable),
        "String");
    }

    /// <summary>
    /// Verify string concatenation (+ operator) works correctly
    /// </summary>
    [Fact]
    public void String_ConcatenationWithPlus_Succeeds()
    {
      var result = CompileAndExecute("\"Hello\" + \" \" + \"World\"", Compiler.Options.Immutable);
      Assert.Equal("Hello World", result.StringValue);
    }

    /// <summary>
    /// Per spec: String + Number → String (string conversion via ToString)
    /// </summary>
    [Fact]
    public void String_ConcatenationWithNumber_ConvertsToString()
    {
      var result = CompileAndExecute("\"Value: \" + 42", Compiler.Options.Immutable);
      Assert.Equal("Value: 42", result.StringValue);
    }

    /// <summary>
    /// Per spec: String + Boolean → String (converts to "true"/"false" lowercase)
    /// </summary>
    [Theory]
    [InlineData("\"Result: \" + true", "Result: true")]
    [InlineData("\"Result: \" + false", "Result: false")]
    public void String_ConcatenationWithBoolean_ConvertsToString(string expression, string expected)
    {
      var result = CompileAndExecute(expression, Compiler.Options.Immutable);
      Assert.Equal(expected, result.StringValue);
    }

    #endregion

    #region String Ordering Comparisons (Per Spec: NOT supported)

    /// <summary>
    /// Per LANGUAGE.md: "<, <=, >, >=: ONLY work with numeric types (int/float)"
    /// "String ordering comparisons are NOT supported"
    /// </summary>
    [Theory]
    [InlineData("\"apple\" < \"banana\"")]
    [InlineData("\"zebra\" > \"apple\"")]
    [InlineData("\"test\" <= \"test\"")]
    [InlineData("\"abc\" >= \"def\"")]
    public void String_OrderingComparison_ThrowsRuntimeException(string expression)
    {
      var compiler = CreateCompiler();
      ErrorTestHelper.AssertRuntimeException(
        () => compiler.Compile(expression, Compiler.Options.Immutable),
        null);
    }

    /// <summary>
    /// Verify that string equality/inequality works (these ARE supported)
    /// </summary>
    [Theory]
    [InlineData("\"hello\" == \"hello\"", true)]
    [InlineData("\"hello\" == \"world\"", false)]
    [InlineData("\"hello\" != \"world\"", true)]
    [InlineData("\"hello\" != \"hello\"", false)]
    public void String_EqualityComparison_Works(string expression, bool expected)
    {
      var result = CompileAndExecute(expression, Compiler.Options.Immutable);
      Assert.Equal(expected, result.BooleanValue);
    }

    #endregion

    #region Compound Assignment Operators

    /// <summary>
    /// Verify that compound assignment operators work
    /// </summary>
    [Theory]
    [InlineData("x += 5", 15)]
    [InlineData("x -= 3", 7)]
    [InlineData("x *= 2", 20)]
    [InlineData("x /= 2", 5)]
    [InlineData("x %= 3", 1)]
    public void CompoundAssignment_SupportedOperators_Work(string expression, int expected)
    {
      var variables = Variables().WithInteger("x", 10).Build();
      var result = CompileAndExecute(expression, Compiler.Options.None, variables);
      Assert.Equal(expected, variables["x"].IntegerValue);
    }

    #endregion

    #region Short-Circuit Evaluation (Per Spec: Required)

    /// <summary>
    /// Per LANGUAGE.md: "&&" operator: if left operand is false, right is not evaluated
    /// </summary>
    [Fact]
    public void BooleanAnd_ShortCircuits_WhenLeftIsFalse()
    {
      var callCount = 0;
      var probe = CustomFunction.Create("probe", (bool value) =>
      {
        callCount++;
        return value;
      });

      var result = CompileAndExecute("false && probe(true)", Compiler.Options.Immutable, null, null, probe);

      Assert.False(result.BooleanValue);
      Assert.Equal(0, callCount); // probe should not be called
    }

    /// <summary>
    /// Per LANGUAGE.md: "||" operator: if left operand is true, right is not evaluated
    /// </summary>
    [Fact]
    public void BooleanOr_ShortCircuits_WhenLeftIsTrue()
    {
      var callCount = 0;
      var probe = CustomFunction.Create("probe", (bool value) =>
      {
        callCount++;
        return value;
      });

      var result = CompileAndExecute("true || probe(false)", Compiler.Options.Immutable, null, null, probe);

      Assert.True(result.BooleanValue);
      Assert.Equal(0, callCount); // probe should not be called
    }

    /// <summary>
    /// Verify that both sides ARE evaluated when short-circuit doesn't trigger
    /// </summary>
    [Fact]
    public void BooleanAnd_EvaluatesBothSides_WhenLeftIsTrue()
    {
      var callCount = 0;
      var probe = CustomFunction.Create("probe", (bool value) =>
      {
        callCount++;
        return value;
      });

      var result = CompileAndExecute("true && probe(false)", Compiler.Options.Immutable, null, null, probe);

      Assert.False(result.BooleanValue);
      Assert.Equal(1, callCount); // probe SHOULD be called
    }

    /// <summary>
    /// Verify that both sides ARE evaluated when short-circuit doesn't trigger
    /// </summary>
    [Fact]
    public void BooleanOr_EvaluatesBothSides_WhenLeftIsFalse()
    {
      var callCount = 0;
      var probe = CustomFunction.Create("probe", (bool value) =>
      {
        callCount++;
        return value;
      });

      var result = CompileAndExecute("false || probe(true)", Compiler.Options.Immutable, null, null, probe);

      Assert.True(result.BooleanValue);
      Assert.Equal(1, callCount); // probe SHOULD be called
    }

    #endregion

    #region Type Coercion Rules (Per Spec)

    // Note: EpsilonScript uses checked arithmetic that throws OverflowException
    // instead of wraparound behavior. This is intentional for safety.

    /// <summary>
    /// Per spec: Mixed type comparisons always throw runtime errors
    /// </summary>
    [Theory]
    [InlineData("\"text\" == 123")]
    [InlineData("true == 1")]
    [InlineData("false == 0")]
    [InlineData("\"true\" == true")]
    public void MixedType_Comparison_ThrowsRuntimeException(string expression)
    {
      var compiler = CreateCompiler();
      ErrorTestHelper.AssertRuntimeException(
        () => compiler.Compile(expression, Compiler.Options.Immutable),
        null);
    }

    /// <summary>
    /// Per spec: Float equality uses ULP-based comparison for precision handling
    /// Very close float values should compare as equal
    /// </summary>
    [Fact]
    public void Float_Equality_UsesULPComparison()
    {
      // These values are very close but not exactly equal in binary floating point
      var result = CompileAndExecute("0.1 + 0.2 == 0.3", Compiler.Options.Immutable);
      // With ULP comparison, this should be true
      Assert.True(result.BooleanValue);
    }

    #endregion

    #region Variable Rules (Per Spec)

    /// <summary>
    /// Per spec: Variables cannot be defined within expressions
    /// Undefined variables should throw runtime error
    /// </summary>
    [Fact]
    public void UndefinedVariable_ThrowsRuntimeException()
    {
      var compiler = CreateCompiler();
      var script = compiler.Compile("undefinedVar + 1");
      ErrorTestHelper.AssertRuntimeException(() => script.Execute(), "Undefined variable");
    }

    /// <summary>
    /// Per spec: In immutable mode, assignments are forbidden
    /// </summary>
    [Fact]
    public void ImmutableMode_Assignment_ThrowsException()
    {
      var variables = Variables().WithInteger("x", 10).Build();
      var compiler = CreateCompiler();
      Assert.ThrowsAny<System.Exception>(() =>
        compiler.Compile("x = 20", Compiler.Options.Immutable, variables));
    }

    /// <summary>
    /// Per spec: Assignment operators require left-hand side to be a variable
    /// This is now caught at parse time, not runtime
    /// </summary>
    [Theory]
    [InlineData("1 = 2")]
    [InlineData("(1 + 2) = 3")]
    [InlineData("true = false")]
    [InlineData("\"literal\" = \"value\"")]
    public void Assignment_ToNonVariable_ThrowsParserException(string expression)
    {
      var compiler = CreateCompiler();
      ErrorTestHelper.AssertParserException(() => compiler.Compile(expression), "Assignment");
    }

    #endregion

    #region Logical Operations (Per Spec: Only boolean values)

    /// <summary>
    /// Per spec: Logical operations only operate on boolean values
    /// </summary>
    [Theory]
    [InlineData("1 && true")]
    [InlineData("true && 1")]
    [InlineData("0 || false")]
    [InlineData("\"yes\" && \"no\"")]
    public void LogicalOperation_OnNonBoolean_ThrowsRuntimeException(string expression)
    {
      var compiler = CreateCompiler();
      ErrorTestHelper.AssertRuntimeException(
        () => compiler.Compile(expression, Compiler.Options.Immutable),
        null);
    }

    /// <summary>
    /// Verify logical operations work with boolean values
    /// </summary>
    [Theory]
    [InlineData("true && true", true)]
    [InlineData("true && false", false)]
    [InlineData("false && true", false)]
    [InlineData("false && false", false)]
    [InlineData("true || true", true)]
    [InlineData("true || false", true)]
    [InlineData("false || true", true)]
    [InlineData("false || false", false)]
    public void LogicalOperation_OnBooleans_Works(string expression, bool expected)
    {
      var result = CompileAndExecute(expression, Compiler.Options.Immutable);
      Assert.Equal(expected, result.BooleanValue);
    }

    /// <summary>
    /// Per spec: Negation only works on booleans
    /// </summary>
    [Theory]
    [InlineData("!1")]
    [InlineData("!0")]
    [InlineData("!\"text\"")]
    [InlineData("!42.5")]
    public void Negation_OnNonBoolean_ThrowsRuntimeException(string expression)
    {
      var compiler = CreateCompiler();
      ErrorTestHelper.AssertRuntimeException(
        () => compiler.Compile(expression, Compiler.Options.Immutable),
        null);
    }

    #endregion
  }
}