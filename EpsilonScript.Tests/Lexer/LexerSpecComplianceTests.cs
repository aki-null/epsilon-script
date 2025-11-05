using System;
using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.Lexer
{
  /// <summary>
  /// Tests to verify compliance with LANGUAGE.md specification
  /// These tests ensure that the lexer correctly rejects invalid syntax per the spec
  /// </summary>
  [Trait("Category", "Unit")]
  [Trait("Component", "Lexer")]
  [Trait("Priority", "Critical")]
  public class LexerSpecComplianceTests : LexerTestBase
  {
    #region Numbers Starting with Decimal Point (Per Spec: Invalid)

    /// <summary>
    /// Per LANGUAGE.md: "Floats MUST start with digits (e.g., ".5" is invalid, must be "0.5")"
    /// </summary>
    [Theory]
    [InlineData(".5")]
    [InlineData(".0")]
    [InlineData(".123")]
    [InlineData(".999")]
    [InlineData(".1234567890")]
    public void Lexer_NumberStartingWithDecimalPoint_Fails(string input)
    {
      AssertLexFails(input);
    }

    /// <summary>
    /// Verify that the correct format (leading zero) works
    /// </summary>
    [Theory]
    [MemberData(nameof(ValidDecimalFormats))]
    internal void Lexer_NumberWithLeadingZero_Succeeds(string input, params Token[] expected)
    {
      AssertLexSucceeds(input, expected);
    }

    public static IEnumerable<object[]> ValidDecimalFormats
    {
      get
      {
        return new[]
        {
          new object[] { "0.5", new Token("0.5", TokenType.Float) },
          new object[] { "0.0", new Token("0.0", TokenType.Float) },
          new object[] { "0.123", new Token("0.123", TokenType.Float) },
          new object[] { "0.999", new Token("0.999", TokenType.Float) },
          new object[] { "0.1234567890", new Token("0.1234567890", TokenType.Float) }
        };
      }
    }

    #endregion

    #region Integer Literals with Exponents (Per Spec: Invalid at lexer level)

    /// <summary>
    /// Per LANGUAGE.md: "Integer literals cannot contain exponent notation (e.g., "2e10" is invalid)"
    /// The lexer rejects this format.
    /// Scientific notation requires a decimal point: "2.0e10" is valid, "2e10" is not.
    ///
    /// Rationale: This restriction enforces clarity between integer and float literals.
    /// While "2e10" could be interpreted as a float that represents a whole number,
    /// the spec requires explicit use of the decimal point to indicate floating-point types.
    /// This makes the type intention clear at the lexical level and avoids ambiguity.
    /// Languages like Python and JavaScript allow "2e10" as a numeric literal, but
    /// EpsilonScript follows a stricter approach similar to some statically-typed languages.
    /// </summary>
    [Theory]
    [InlineData("2e10")]
    [InlineData("5E5")]
    [InlineData("100e2")]
    [InlineData("1e0")]
    public void Lexer_IntegerLiteralWithExponent_ThrowsException(string input)
    {
      // Per specification, integer literals without decimal points cannot use exponent notation
      // This enforces explicit float type indication via the decimal point
      AssertLexFails(input);
    }

    /// <summary>
    /// Verify that float exponent notation (with decimal point) works correctly
    /// </summary>
    [Theory]
    [MemberData(nameof(ValidExponentFormats))]
    internal void Lexer_FloatWithExponent_Succeeds(string input, params Token[] expected)
    {
      AssertLexSucceeds(input, expected);
    }

    public static IEnumerable<object[]> ValidExponentFormats
    {
      get
      {
        return new[]
        {
          new object[] { "2.0e10", new Token("2.0e10", TokenType.Float) },
          new object[] { "5.0E5", new Token("5.0E5", TokenType.Float) },
          new object[] { "100.0e2", new Token("100.0e2", TokenType.Float) },
          new object[] { "1.e0", new Token("1.e0", TokenType.Float) }
        };
      }
    }

    #endregion

    #region Leading Zeros Behavior

    /// <summary>
    /// Test behavior of leading zeros in integer literals
    /// </summary>
    [Theory]
    [MemberData(nameof(LeadingZeroData))]
    internal void Lexer_IntegerWithLeadingZeros_TokenizesAsInteger(string input, params Token[] expected)
    {
      AssertLexSucceeds(input, expected);
    }

    public static IEnumerable<object[]> LeadingZeroData
    {
      get
      {
        return new[]
        {
          new object[] { "0", new Token("0", TokenType.Integer) },
          new object[] { "00", new Token("00", TokenType.Integer) },
          new object[] { "007", new Token("007", TokenType.Integer) },
          new object[] { "0123", new Token("0123", TokenType.Integer) },
          new object[] { "00000", new Token("00000", TokenType.Integer) }
        };
      }
    }

    #endregion

    #region String Escape Sequences (Per Spec: Not Supported)

    /// <summary>
    /// Verify that strings without escapes work correctly
    /// </summary>
    [Theory]
    [MemberData(nameof(ValidStringData))]
    internal void Lexer_StringWithoutEscapes_Succeeds(string input, params Token[] expected)
    {
      AssertLexSucceeds(input, expected);
    }

    public static IEnumerable<object[]> ValidStringData
    {
      get
      {
        return new[]
        {
          new object[] { "\"hello world\"", new Token("\"hello world\"", TokenType.String) },
          new object[] { "\"test\"", new Token("\"test\"", TokenType.String) },
          new object[] { "\"\"", new Token("\"\"", TokenType.String) },
          new object[] { "\"with spaces   \"", new Token("\"with spaces   \"", TokenType.String) }
        };
      }
    }

    /// <summary>
    /// Unterminated strings should fail
    /// </summary>
    [Theory]
    [InlineData("\"hello")]
    [InlineData("\"unterminated")]
    [InlineData("\"")]
    public void Lexer_UnterminatedString_Fails(string input)
    {
      AssertLexFails(input);
    }

    #endregion

    #region Empty Input

    /// <summary>
    /// Test that empty input is handled correctly
    /// </summary>
    [Fact]
    public void Lexer_EmptyInput_ProducesNoTokens()
    {
      AssertLexSucceeds("", new Token[] { });
    }

    /// <summary>
    /// Test that whitespace-only input produces no tokens
    /// </summary>
    [Theory]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("  \t  \n  ")]
    public void Lexer_WhitespaceOnlyInput_ProducesNoTokens(string input)
    {
      AssertLexSucceeds(input, new Token[] { });
    }

    #endregion

    #region Float Format Edge Cases

    /// <summary>
    /// Per spec: "Float fractional part is optional (e.g., "10." is valid)"
    /// </summary>
    [Theory]
    [MemberData(nameof(TrailingDecimalData))]
    internal void Lexer_FloatWithTrailingDecimal_Succeeds(string input, params Token[] expected)
    {
      AssertLexSucceeds(input, expected);
    }

    public static IEnumerable<object[]> TrailingDecimalData
    {
      get
      {
        return new[]
        {
          new object[] { "1.", new Token("1.", TokenType.Float) },
          new object[] { "10.", new Token("10.", TokenType.Float) },
          new object[] { "999.", new Token("999.", TokenType.Float) },
          new object[] { "0.", new Token("0.", TokenType.Float) }
        };
      }
    }

    #endregion

    #region Maximum Length Limits

    /// <summary>
    /// Test very long numeric literals
    /// </summary>
    [Fact]
    public void Lexer_VeryLongInteger_Tokenizes()
    {
      var longNumber = new string('9', 100);
      AssertLexSucceeds(longNumber, new[] { new Token(longNumber, TokenType.Integer) });
    }

    /// <summary>
    /// Test very long identifiers
    /// </summary>
    [Fact]
    public void Lexer_VeryLongIdentifier_Tokenizes()
    {
      var longId = new string('a', 1000);
      AssertLexSucceeds(longId, new[] { new Token(longId, TokenType.Identifier) });
    }

    /// <summary>
    /// Test very long strings
    /// </summary>
    [Fact]
    public void Lexer_VeryLongString_Tokenizes()
    {
      var longString = "\"" + new string('x', 10000) + "\"";
      AssertLexSucceeds(longString, new[] { new Token(longString, TokenType.String) });
    }

    #endregion

    #region Identifier Edge Cases

    /// <summary>
    /// Test identifiers with dots (member access)
    /// </summary>
    [Theory]
    [MemberData(nameof(DottedIdentifierData))]
    internal void Lexer_IdentifierWithDots_Tokenizes(string input, params Token[] expected)
    {
      AssertLexSucceeds(input, expected);
    }

    public static IEnumerable<object[]> DottedIdentifierData
    {
      get
      {
        return new[]
        {
          new object[] { "a.b", new Token("a.b", TokenType.Identifier) },
          new object[] { "var.prop", new Token("var.prop", TokenType.Identifier) },
          new object[] { "obj.method.call", new Token("obj.method.call", TokenType.Identifier) },
          new object[] { "_private.field", new Token("_private.field", TokenType.Identifier) }
        };
      }
    }

    /// <summary>
    /// Test identifiers starting with underscore
    /// </summary>
    [Theory]
    [InlineData("_var")]
    [InlineData("_")]
    [InlineData("__")]
    [InlineData("_123")]
    [InlineData("_test_var")]
    public void Lexer_IdentifierStartingWithUnderscore_Tokenizes(string input)
    {
      AssertLexSucceeds(input, new[] { new Token(input, TokenType.Identifier) });
    }

    /// <summary>
    /// Test identifiers with numbers (not at start)
    /// </summary>
    [Theory]
    [InlineData("var1")]
    [InlineData("test123")]
    [InlineData("a1b2c3")]
    [InlineData("value_42")]
    public void Lexer_IdentifierWithNumbers_Tokenizes(string input)
    {
      AssertLexSucceeds(input, new[] { new Token(input, TokenType.Identifier) });
    }

    #endregion
  }
}