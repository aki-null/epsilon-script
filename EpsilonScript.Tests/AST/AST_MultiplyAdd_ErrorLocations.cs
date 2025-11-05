using Xunit;
using EpsilonScript.Tests.ScriptSystem;

namespace EpsilonScript.Tests.AST
{
  [Trait("Category", "Unit")]
  [Trait("Component", "AST")]
  [Trait("Feature", "ErrorLocation")]
  public class AST_MultiplyAdd_ErrorLocations : ScriptTestBase
  {
    /// <summary>
    /// Tests that verify runtime errors from MultiplyAddNode point to the precise
    /// operator location that caused the error (multiply vs add/subtract).
    ///
    /// Note: Line and character positions are stored as 0-indexed internally (LSP convention),
    /// but displayed to users as 1-indexed (e.g., "line 1, column 8" for LineNumber=0, CharacterIndex=7).
    /// </summary>

    #region Multiply Operation Errors

    [Fact]
    public void MA_MultiplyError_BooleanInMultiply_PointsToMultiplyOperator()
    {
      // Script: (x * true) + 5
      //            ^
      // Error should point to * operator at 0-indexed position 3
      var vars = Variables()
        .WithInteger("x", 2)
        .Build();

      var ex = Assert.Throws<RuntimeException>(() =>
        CompileAndExecute("(x * true) + 5", Compiler.Options.None, vars));

      // Verify error points to multiply operator location
      Assert.Equal(0, ex.Location.LineNumber);
      Assert.Equal(3, ex.Location.CharacterIndex); // 0-indexed: (x * true)
      Assert.Contains("Boolean", ex.Message);
      Assert.Contains("arithmetic operations", ex.Message);
    }

    [Fact]
    public void MA_MultiplyError_StringInMultiply_PointsToMultiplyOperator()
    {
      // Script: 5 + (x * "hello")
      //               ^
      // Error should point to * operator at 0-indexed position 7
      var vars = Variables()
        .WithInteger("x", 2)
        .Build();

      var ex = Assert.Throws<RuntimeException>(() =>
        CompileAndExecute("5 + (x * \"hello\")", Compiler.Options.None, vars));

      // Verify error points to multiply operator location
      Assert.Equal(0, ex.Location.LineNumber);
      Assert.Equal(7, ex.Location.CharacterIndex); // 0-indexed: 5 + (x * "hello")
      Assert.Contains("Multiply operation", ex.Message);
      Assert.Contains("numeric", ex.Message);
    }

    [Fact]
    public void MA_MultiplyError_LeftMultiplyPattern_CorrectLocation()
    {
      // Pattern: (a * b) + c
      // Script:  (true * 5) + 10
      //                ^
      // Error at multiply operator at 0-indexed position 6
      var ex = Assert.Throws<RuntimeException>(() =>
        CompileAndExecute("(true * 5) + 10"));

      Assert.Equal(0, ex.Location.LineNumber);
      Assert.Equal(6, ex.Location.CharacterIndex); // 0-indexed: (true * 5)
      Assert.Contains("Boolean", ex.Message);
    }

    [Fact]
    public void MA_MultiplyError_RightMultiplyPattern_CorrectLocation()
    {
      // Pattern: c + (a * b)
      // Script:  10 + (5 * false)
      //                  ^
      // Error at multiply operator at 0-indexed position 8
      var ex = Assert.Throws<RuntimeException>(() =>
        CompileAndExecute("10 + (5 * false)"));

      Assert.Equal(0, ex.Location.LineNumber);
      Assert.Equal(8, ex.Location.CharacterIndex); // 0-indexed: 10 + (5 * false)
      Assert.Contains("Boolean", ex.Message);
    }

    [Fact]
    public void MA_MultiplyError_SubtractPattern_CorrectLocation()
    {
      // Pattern: (a * b) - c
      // Script:  (s * 5) - 10
      //             ^
      // Error at multiply operator at 0-indexed position 3
      var vars = Variables()
        .WithString("s", "text")
        .Build();

      var ex = Assert.Throws<RuntimeException>(() =>
        CompileAndExecute("(s * 5) - 10", Compiler.Options.None, vars));

      Assert.Equal(0, ex.Location.LineNumber);
      Assert.Equal(3, ex.Location.CharacterIndex); // 0-indexed: (s * 5)
      Assert.Contains("Multiply", ex.Message);
    }

    #endregion

    #region Add/Subtract Operation Errors

    [Fact]
    public void MA_AddError_BooleanInAddend_PointsToAddOperator()
    {
      // Script: (2 * 3) + true
      //                 ^
      // Error should point to + operator at 0-indexed position 8
      var ex = Assert.Throws<RuntimeException>(() =>
        CompileAndExecute("(2 * 3) + true"));

      // Verify error points to add operator location
      Assert.Equal(0, ex.Location.LineNumber);
      Assert.Equal(8, ex.Location.CharacterIndex); // 0-indexed: (2 * 3) + true
      Assert.Contains("Boolean", ex.Message);
    }

    [Fact]
    public void MA_AddError_StringOnRight_PointsToAddOperator()
    {
      // Script: (2 * 3) + "hello"
      //                 ^
      // Error should point to + operator at 0-indexed position 8
      // String concatenation only allowed when string is on LEFT
      var ex = Assert.Throws<RuntimeException>(() =>
        CompileAndExecute("(2 * 3) + \"hello\""));

      Assert.Equal(0, ex.Location.LineNumber);
      Assert.Equal(8, ex.Location.CharacterIndex); // 0-indexed: (2 * 3) + "hello"
      Assert.Contains("numeric", ex.Message);
    }

    [Fact]
    public void MA_SubtractError_StringInAddend_PointsToSubtractOperator()
    {
      // Script: (2 * 3) - "test"
      //                 ^
      // Error should point to - operator at 0-indexed position 8
      var ex = Assert.Throws<RuntimeException>(() =>
        CompileAndExecute("(2 * 3) - \"test\""));

      Assert.Equal(0, ex.Location.LineNumber);
      Assert.Equal(8, ex.Location.CharacterIndex); // 0-indexed: (2 * 3) - "test"
      Assert.Contains("numeric", ex.Message);
    }

    [Fact]
    public void MA_SubtractError_StringLeftSide_PointsToSubtractOperator()
    {
      // Pattern: c - (a * b) where c is string
      // Script:  "hello" - (2 * 3)
      //                  ^
      // Error at subtract operator at 0-indexed position 8
      var ex = Assert.Throws<RuntimeException>(() =>
        CompileAndExecute("\"hello\" - (2 * 3)"));

      Assert.Equal(0, ex.Location.LineNumber);
      Assert.Equal(8, ex.Location.CharacterIndex); // 0-indexed: "hello" - (2 * 3)
      Assert.Contains("String", ex.Message);
      Assert.Contains("subtraction", ex.Message);
    }

    [Fact]
    public void MA_AddError_RightPattern_BooleanAddend_CorrectLocation()
    {
      // Pattern: c + (a * b)
      // Script:  true + (2 * 3)
      //               ^
      // Error at add operator at 0-indexed position 5
      var ex = Assert.Throws<RuntimeException>(() =>
        CompileAndExecute("true + (2 * 3)"));

      Assert.Equal(0, ex.Location.LineNumber);
      Assert.Equal(5, ex.Location.CharacterIndex); // 0-indexed: true + (2 * 3)
      Assert.Contains("Boolean", ex.Message);
    }

    #endregion

    #region Complex Expressions - Verify Correct Operator Attribution

    [Fact]
    public void MA_ComplexExpression_MultiplyErrorNotAddError()
    {
      // Script: x + (y * bad) + z
      //                ^
      // Error should point to * operator at 0-indexed position 7, NOT + operators
      var vars = Variables()
        .WithInteger("x", 1)
        .WithInteger("y", 2)
        .WithString("bad", "invalid")
        .WithInteger("z", 3)
        .Build();

      var ex = Assert.Throws<RuntimeException>(() =>
        CompileAndExecute("x + (y * bad) + z", Compiler.Options.None, vars));

      // Should point to the multiply operator inside parentheses
      Assert.Equal(0, ex.Location.LineNumber);
      Assert.Equal(7, ex.Location.CharacterIndex); // 0-indexed: x + (y * bad)
      // Error message should indicate the problem is with the multiply operation
      Assert.Contains("arithmetic operation", ex.Message);
      Assert.Contains("numeric values", ex.Message);
    }

    [Fact]
    public void MA_WithSpaces_LocationStillAccurate()
    {
      // Script with extra spaces: ( 2  *  3 )  +  true
      //                                        ^
      // Error should point to + operator at 0-indexed position 13
      var ex = Assert.Throws<RuntimeException>(() =>
        CompileAndExecute("( 2  *  3 )  +  true"));

      Assert.Equal(0, ex.Location.LineNumber);
      Assert.Equal(13, ex.Location.CharacterIndex); // 0-indexed: ( 2  *  3 )  +  true
      Assert.Contains("Boolean", ex.Message);
    }

    #endregion

    #region Multiline Scripts

    [Fact]
    public void MA_Multiline_ErrorOnSecondLine_CorrectLineAndColumn()
    {
      // Script (internally 0-indexed lines):
      // Line 0: x = 5;
      // Line 1: y = (2 * bad) + 10
      //                ^
      // Error at line 1, * operator at character 7 (displayed as "line 2, column 8")
      var vars = Variables()
        .WithInteger("x", 0)
        .WithInteger("y", 0)
        .WithString("bad", "invalid")
        .Build();

      var ex = Assert.Throws<RuntimeException>(() =>
        CompileAndExecute("x = 5;\ny = (2 * bad) + 10", Compiler.Options.None, vars));

      Assert.Equal(1, ex.Location.LineNumber);
      Assert.Equal(7, ex.Location.CharacterIndex); // 0-indexed on line 1 (second line): y = (2 * bad)
      // Error message should indicate the problem is with the multiply operation
      Assert.Contains("arithmetic operation", ex.Message);
      Assert.Contains("numeric values", ex.Message);
    }

    [Fact]
    public void MA_Multiline_AddErrorOnThirdLine_CorrectLocation()
    {
      // Script (internally 0-indexed lines):
      // Line 0: x = 1;
      // Line 1: y = 2;
      // Line 2: z = (x * y) + false
      //                     ^
      // Error at line 2, + operator at character 12 (displayed as "line 3, column 13")
      var vars = Variables()
        .WithInteger("x", 0)
        .WithInteger("y", 0)
        .WithInteger("z", 0)
        .Build();

      var ex = Assert.Throws<RuntimeException>(() =>
        CompileAndExecute("x = 1;\ny = 2;\nz = (x * y) + false", Compiler.Options.None, vars));

      Assert.Equal(2, ex.Location.LineNumber);
      Assert.Equal(12, ex.Location.CharacterIndex); // 0-indexed on line 2 (third line): z = (x * y) + false
      Assert.Contains("Boolean", ex.Message);
    }

    #endregion

    #region NoAlloc Mode Errors

    [Fact]
    public void MA_NoAllocError_StringConcatenation_PointsToAddOperator()
    {
      // In NoAlloc mode, string concatenation should error at + operator
      // Script: s + (2 * 3)
      //           ^
      // Error at + operator at 0-indexed position 2
      var vars = Variables()
        .WithString("s", "result=")
        .Build();

      var ex = Assert.Throws<RuntimeException>(() =>
        CompileAndExecute("s + (2 * 3)", Compiler.Options.NoAlloc, vars));

      Assert.Equal(0, ex.Location.LineNumber);
      Assert.Equal(2, ex.Location.CharacterIndex); // 0-indexed: s + (2 * 3)
      Assert.Contains("NoAlloc", ex.Message);
      Assert.Contains("String concatenation", ex.Message);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void MA_NestedMultiplyAdd_InnerError_CorrectLocation()
    {
      // Script: ((2 * true) + 5) + 10
      //             ^
      // Error at inner multiply operator at 0-indexed position 4
      var ex = Assert.Throws<RuntimeException>(() =>
        CompileAndExecute("((2 * true) + 5) + 10"));

      Assert.Equal(0, ex.Location.LineNumber);
      Assert.Equal(4, ex.Location.CharacterIndex); // 0-indexed: ((2 * true) + 5)
      Assert.Contains("Boolean", ex.Message);
    }

    [Fact]
    public void MA_NestedMultiplyAdd_OuterError_CorrectLocation()
    {
      // Script: ((2 * 3) + 5) + "bad"
      //                       ^
      // Error at outer add operator at 0-indexed position 14
      var ex = Assert.Throws<RuntimeException>(() =>
        CompileAndExecute("((2 * 3) + 5) + \"bad\""));

      Assert.Equal(0, ex.Location.LineNumber);
      Assert.Equal(14, ex.Location.CharacterIndex); // 0-indexed: ((2 * 3) + 5) + "bad"
      Assert.Contains("numeric", ex.Message);
    }

    #endregion
  }
}