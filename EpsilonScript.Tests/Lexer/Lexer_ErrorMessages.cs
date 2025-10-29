using EpsilonScript.Lexer;
using System;
using Xunit;

namespace EpsilonScript.Tests.Lexer
{
  /// <summary>
  /// Tests to verify that lexer error messages are informative
  /// </summary>
  [Trait("Category", "Unit")]
  [Trait("Component", "Lexer")]
  [Trait("Priority", "High")]
  public class Lexer_ErrorMessages
  {
    /// <summary>
    /// Helper method to execute lexer and capture the expected exception
    /// </summary>
    private static LexerException ExpectLexerException(string input)
    {
      return Assert.Throws<LexerException>(() =>
      {
        var lexer = new EpsilonScript.Lexer.Lexer();
        var tokenReader = new TestInfrastructure.Fakes.TestTokenReader();
        lexer.Execute(input.AsMemory(), tokenReader);
      });
    }

    [Fact]
    public void InvalidCharacter_AtSign_IsInErrorMessage()
    {
      var ex = ExpectLexerException("test @ value");

      // Error should mention the invalid character
      Assert.Contains("@", ex.Message);
    }

    [Fact]
    public void InvalidCharacter_OpenBrace_IsInErrorMessage()
    {
      var ex = ExpectLexerException("{");

      Assert.Contains("{", ex.Message);
    }

    [Fact]
    public void UnterminatedString_ProvidesErrorMessage()
    {
      var ex = ExpectLexerException("\"unterminated");

      // Should match the exact error message from Lexer.cs
      Assert.Equal("String literal does not have a closing double quotation mark", ex.Message);
    }

    [Fact]
    public void InvalidExponent_MissingNumber_ProvidesErrorMessage()
    {
      var ex = ExpectLexerException("1.0e");

      // Should match the exact error message from Lexer.cs
      Assert.Equal("Float exponent requires an integer value (e.g., '1.0e10', not '1.0e')", ex.Message);
    }

    [Fact]
    public void InvalidExponent_OnlySign_ProvidesErrorMessage()
    {
      var ex = ExpectLexerException("1.0e+");

      // Should match the exact error message from Lexer.cs
      Assert.Equal("Float exponent requires an integer value (e.g., '1.0e10', not '1.0e')", ex.Message);
    }

    [Fact]
    public void IntegerWithExponent_WithoutDecimal_Fails()
    {
      var ex = ExpectLexerException("2e10");

      // Should match the exact error message from Lexer.cs
      Assert.Equal("Float exponent notation requires decimal point (e.g., use '2.0e10' instead of '2e10')", ex.Message);
    }

    [Theory]
    [InlineData("#")]
    [InlineData("$")]
    [InlineData("^")]
    [InlineData("~")]
    [InlineData("`")]
    public void VariousInvalidCharacters_EachProvidesSpecificError(string invalidChar)
    {
      var ex = ExpectLexerException(invalidChar);

      // Each error should mention the specific invalid character
      Assert.Contains(invalidChar, ex.Message);
    }

    [Fact]
    public void SingleAmpersand_ProvidesErrorMessage()
    {
      var ex = ExpectLexerException("true & false");

      // Should match the exact error message from Lexer.cs
      Assert.Equal("AND boolean operator requires two ampersand characters '&&'", ex.Message);
    }

    [Fact]
    public void SinglePipe_ProvidesErrorMessage()
    {
      var ex = ExpectLexerException("true | false");

      // Should match the exact error message from Lexer.cs
      Assert.Equal("OR boolean operator requires two vertical bar characters '||'", ex.Message);
    }
  }
}