using EpsilonScript.Function;
using Xunit;

namespace EpsilonScript.Tests.ScriptSystem
{
  /// <summary>
  /// Tests that verify tuples can ONLY be used as function parameters.
  /// Invalid tuple usage (commas outside function calls) should throw ParserException during compilation.
  ///
  /// Background: Tuples are created by comma operators and are intended exclusively for passing
  /// multiple parameters to functions. Any other use of commas/tuples is a syntax error.
  /// </summary>
  [Trait("Category", "Integration")]
  [Trait("Component", "Parser")]
  public class ScriptTupleValidationTests : ScriptTestBase
  {
    #region Valid Tuple Usage - These should compile and execute successfully

    [Fact]
    public void FunctionWithMultipleParameters_ValidTupleUsage_CompilesSuccessfully()
    {
      var compiler = CreateCompiler();

      // This is the ONLY valid use of commas - function parameters
      var script = compiler.Compile("max(1, 2)", Compiler.Options.Immutable);
      script.Execute();

      Assert.Equal(2, script.IntegerValue);
    }

    [Fact]
    public void FunctionWithThreeParameters_ValidTupleUsage_CompilesSuccessfully()
    {
      var compiler = CreateCompiler();
      compiler.AddCustomFunction(CustomFunction.Create("sum", (int a, int b, int c) => a + b + c));

      var script = compiler.Compile("sum(1, 2, 3)", Compiler.Options.Immutable);
      script.Execute();

      Assert.Equal(6, script.IntegerValue);
    }

    [Fact]
    public void FunctionWithExpressionParameters_ValidTupleUsage_CompilesSuccessfully()
    {
      var compiler = CreateCompiler();

      var script = compiler.Compile("max(1 + 2, 3 * 4)", Compiler.Options.Immutable);
      script.Execute();

      Assert.Equal(12, script.IntegerValue);
    }

    #endregion

    #region Invalid: Standalone Tuples - Should throw ParserException

    [Fact]
    public void StandaloneTuple_ThrowsParserException()
    {
      var compiler = CreateCompiler();

      var ex = Assert.Throws<ParserException>(() =>
        compiler.Compile("(1, 2)", Compiler.Options.Immutable));

      Assert.Contains("Comma can only be used to separate function parameters", ex.Message);
      Assert.Contains("column 3", ex.Message); // Points to the comma
    }

    [Fact]
    public void ThreeElementTuple_ThrowsParserException()
    {
      var compiler = CreateCompiler();

      var ex = Assert.Throws<ParserException>(() =>
        compiler.Compile("(1, 2, 3)", Compiler.Options.Immutable));

      Assert.Contains("Comma can only be used to separate function parameters", ex.Message);
    }

    [Fact]
    public void NestedTuple_ThrowsParserException()
    {
      var compiler = CreateCompiler();

      var ex = Assert.Throws<ParserException>(() =>
        compiler.Compile("(1, (2, 3))", Compiler.Options.Immutable));

      Assert.Contains("Comma can only be used to separate function parameters", ex.Message);
    }

    #endregion

    #region Invalid: Tuples in Assignments - Should throw ParserException

    [Fact]
    public void TupleAssignedToVariable_ThrowsParserException()
    {
      var compiler = CreateCompiler();
      var variables = Variables().WithInteger("x", 0).Build();

      var ex = Assert.Throws<ParserException>(() =>
        compiler.Compile("x = (1, 2)"));

      Assert.Contains("Comma can only be used to separate function parameters", ex.Message);
    }

    [Fact]
    public void TupleWithExpressionsAssignedToVariable_ThrowsParserException()
    {
      var compiler = CreateCompiler();
      var variables = Variables().WithInteger("x", 0).Build();

      var ex = Assert.Throws<ParserException>(() =>
        compiler.Compile("x = (1 + 2, 3 * 4)"));

      Assert.Contains("Comma can only be used to separate function parameters", ex.Message);
    }

    [Fact]
    public void TupleWithVariablesAssignedToVariable_ThrowsParserException()
    {
      var compiler = CreateCompiler();
      var variables = Variables()
        .WithInteger("a", 1)
        .WithInteger("b", 2)
        .WithInteger("x", 0)
        .Build();

      var ex = Assert.Throws<ParserException>(() =>
        compiler.Compile("x = (a, b)"));

      Assert.Contains("Comma can only be used to separate function parameters", ex.Message);
    }

    #endregion

    #region Invalid: Tuples in Arithmetic Operations - Should throw ParserException

    [Fact]
    public void TupleInAddition_ThrowsParserException()
    {
      var compiler = CreateCompiler();

      var ex = Assert.Throws<ParserException>(() =>
        compiler.Compile("(1, 2) + 3", Compiler.Options.Immutable));

      Assert.Contains("Comma can only be used to separate function parameters", ex.Message);
    }

    [Fact]
    public void TupleInSubtraction_ThrowsParserException()
    {
      var compiler = CreateCompiler();

      var ex = Assert.Throws<ParserException>(() =>
        compiler.Compile("5 - (1, 2)", Compiler.Options.Immutable));

      Assert.Contains("Comma can only be used to separate function parameters", ex.Message);
    }

    [Fact]
    public void TupleInMultiplication_ThrowsParserException()
    {
      var compiler = CreateCompiler();

      var ex = Assert.Throws<ParserException>(() =>
        compiler.Compile("(1, 2) * 3", Compiler.Options.Immutable));

      Assert.Contains("Comma can only be used to separate function parameters", ex.Message);
    }

    #endregion

    #region Invalid: Tuples in Boolean Operations - Should throw ParserException

    [Fact]
    public void TupleInBooleanAnd_ThrowsParserException()
    {
      var compiler = CreateCompiler();

      var ex = Assert.Throws<ParserException>(() =>
        compiler.Compile("(1, 2) && true", Compiler.Options.Immutable));

      Assert.Contains("Comma can only be used to separate function parameters", ex.Message);
    }

    [Fact]
    public void TupleInBooleanOr_ThrowsParserException()
    {
      var compiler = CreateCompiler();

      var ex = Assert.Throws<ParserException>(() =>
        compiler.Compile("false || (1, 2)", Compiler.Options.Immutable));

      Assert.Contains("Comma can only be used to separate function parameters", ex.Message);
    }

    [Fact]
    public void TupleInNegation_ThrowsParserException()
    {
      var compiler = CreateCompiler();

      var ex = Assert.Throws<ParserException>(() =>
        compiler.Compile("!(1, 2)", Compiler.Options.Immutable));

      Assert.Contains("Comma can only be used to separate function parameters", ex.Message);
    }

    #endregion

    #region Invalid: Tuples in Comparisons - Should throw ParserException

    [Fact]
    public void TupleInEquality_ThrowsParserException()
    {
      var compiler = CreateCompiler();

      var ex = Assert.Throws<ParserException>(() =>
        compiler.Compile("(1, 2) == (3, 4)", Compiler.Options.Immutable));

      Assert.Contains("Comma can only be used to separate function parameters", ex.Message);
    }

    [Fact]
    public void TupleInLessThan_ThrowsParserException()
    {
      var compiler = CreateCompiler();

      var ex = Assert.Throws<ParserException>(() =>
        compiler.Compile("(1, 2) < 3", Compiler.Options.Immutable));

      Assert.Contains("Comma can only be used to separate function parameters", ex.Message);
    }

    [Fact]
    public void TupleInGreaterThan_ThrowsParserException()
    {
      var compiler = CreateCompiler();

      var ex = Assert.Throws<ParserException>(() =>
        compiler.Compile("5 > (1, 2)", Compiler.Options.Immutable));

      Assert.Contains("Comma can only be used to separate function parameters", ex.Message);
    }

    #endregion

    #region Invalid: Tuples in Unary Operations - Should throw ParserException

    [Fact]
    public void TupleWithUnaryPlus_ThrowsParserException()
    {
      var compiler = CreateCompiler();

      var ex = Assert.Throws<ParserException>(() =>
        compiler.Compile("+(1, 2)", Compiler.Options.Immutable));

      Assert.Contains("Comma can only be used to separate function parameters", ex.Message);
    }

    [Fact]
    public void TupleWithUnaryMinus_ThrowsParserException()
    {
      var compiler = CreateCompiler();

      var ex = Assert.Throws<ParserException>(() =>
        compiler.Compile("-(1, 2)", Compiler.Options.Immutable));

      Assert.Contains("Comma can only be used to separate function parameters", ex.Message);
    }

    #endregion

    #region Invalid: Tuples in Sequences - Should throw ParserException

    [Fact]
    public void TupleAsSequenceResult_ThrowsParserException()
    {
      var compiler = CreateCompiler();
      var variables = Variables()
        .WithInteger("x", 0)
        .Build();

      var ex = Assert.Throws<ParserException>(() =>
        compiler.Compile("x = 10; (1, 2)"));

      Assert.Contains("Comma can only be used to separate function parameters", ex.Message);
    }

    [Fact]
    public void TupleInMiddleOfSequence_ThrowsParserException()
    {
      var compiler = CreateCompiler();
      var variables = Variables()
        .WithInteger("x", 0)
        .WithInteger("y", 0)
        .Build();

      var ex = Assert.Throws<ParserException>(() =>
        compiler.Compile("x = 5; (1, 2); y = 10"));

      Assert.Contains("Comma can only be used to separate function parameters", ex.Message);
    }

    #endregion

    #region Error Message Quality - Verify helpful error messages

    [Fact]
    public void TupleError_IncludesColumnNumber()
    {
      var compiler = CreateCompiler();

      var ex = Assert.Throws<ParserException>(() =>
        compiler.Compile("x = (1, 2)"));

      // Should point to the comma location
      Assert.Matches("column \\d+", ex.Message);
    }

    [Fact]
    public void TupleError_MentionsFunctionParameters()
    {
      var compiler = CreateCompiler();

      var ex = Assert.Throws<ParserException>(() =>
        compiler.Compile("(1, 2)"));

      Assert.Contains("function parameters", ex.Message);
    }

    [Fact]
    public void TupleError_MentionsTuples()
    {
      var compiler = CreateCompiler();

      var ex = Assert.Throws<ParserException>(() =>
        compiler.Compile("(1, 2)"));

      Assert.Contains("function parameters", ex.Message);
    }

    #endregion
  }
}