using System;
using Xunit;
using EpsilonScript.Tests.ScriptSystem;

namespace EpsilonScript.Tests.AST
{
  [Trait("Category", "Unit")]
  [Trait("Component", "AST")]
  public class AST_MultiplyAdd_StringBehavior : ScriptTestBase
  {
    // These tests verify that MA optimization maintains exact behavior compatibility
    // with the original two-node ArithmeticNode implementation, especially for strings

    [Fact]
    public void MA_NumericMultiply_PlusString_ThrowsError()
    {
      // (2 * 3) + "abc" should throw error - original behavior only allows string on LEFT
      var vars = Variables()
        .WithInteger("a", 2)
        .WithInteger("b", 3)
        .WithString("c", "abc")
        .Build();

      var ex = Assert.Throws<RuntimeException>(() =>
        CompileAndExecute("(a * b) + c", Compiler.Options.None, vars));

      Assert.Contains("numeric", ex.Message);
    }

    [Fact]
    public void MA_String_PlusNumericMultiply_Concatenates()
    {
      // "abc" + (2 * 3) should produce "abc6" via string concatenation
      var vars = Variables()
        .WithInteger("a", 2)
        .WithInteger("b", 3)
        .WithString("c", "abc")
        .Build();

      var result = CompileAndExecute("c + (a * b)", Compiler.Options.None, vars);

      Assert.Equal("abc6", result.StringValue);
      Assert.Equal(Type.String, result.Type);
    }

    [Fact]
    public void MA_String_MinusNumericMultiply_ThrowsError()
    {
      // "abc" - (2 * 3) should throw an error (strings don't support subtraction)
      var vars = Variables()
        .WithInteger("a", 2)
        .WithInteger("b", 3)
        .WithString("c", "abc")
        .Build();

      var ex = Assert.Throws<RuntimeException>(() =>
        CompileAndExecute("c - (a * b)", Compiler.Options.None, vars));

      // Should error because strings don't support subtraction
      Assert.True(ex.Message.Contains("String") || ex.Message.Contains("numeric"));
    }

    [Fact]
    public void MA_String_PlusNumericMultiply_WithFloat()
    {
      // "result=" + (2.5 * 4.0) should produce "result=10" (or "result=10.0")
      var vars = Variables()
        .WithFloat("a", 2.5f)
        .WithFloat("b", 4.0f)
        .WithString("c", "result=")
        .Build();

      var result = CompileAndExecute("c + (a * b)", Compiler.Options.None, vars);

      Assert.Equal(Type.String, result.Type);
      Assert.StartsWith("result=", result.StringValue);
      Assert.Contains("10", result.StringValue);
    }

    [Fact]
    public void MA_String_PlusZeroMultiply()
    {
      // "value=" + (0 * 999) should produce "value=0"
      var vars = Variables()
        .WithInteger("a", 0)
        .WithInteger("b", 999)
        .WithString("c", "value=")
        .Build();

      var result = CompileAndExecute("c + (a * b)", Compiler.Options.None, vars);

      Assert.Equal("value=0", result.StringValue);
      Assert.Equal(Type.String, result.Type);
    }

    [Fact]
    public void MA_String_PlusNegativeMultiply()
    {
      // "result=" + (-2 * 3) should produce "result=-6"
      var vars = Variables()
        .WithInteger("a", -2)
        .WithInteger("b", 3)
        .WithString("c", "result=")
        .Build();

      var result = CompileAndExecute("c + (a * b)", Compiler.Options.None, vars);

      Assert.Equal("result=-6", result.StringValue);
      Assert.Equal(Type.String, result.Type);
    }

    [Fact]
    public void MA_String_PlusFunctionMultiply()
    {
      // "answer=" + (abs(-5) * 3) should produce "answer=15"
      var vars = Variables()
        .WithInteger("a", -5)
        .WithInteger("b", 3)
        .WithString("c", "answer=")
        .Build();

      var result = CompileAndExecute("c + (abs(a) * b)", Compiler.Options.None, vars);

      Assert.Equal("answer=15", result.StringValue);
      Assert.Equal(Type.String, result.Type);
    }

    [Fact]
    public void MA_EmptyString_PlusNumericMultiply()
    {
      // "" + (2 * 3) should produce "6"
      var vars = Variables()
        .WithInteger("a", 2)
        .WithInteger("b", 3)
        .WithString("c", "")
        .Build();

      var result = CompileAndExecute("c + (a * b)", Compiler.Options.None, vars);

      Assert.Equal("6", result.StringValue);
      Assert.Equal(Type.String, result.Type);
    }
  }
}