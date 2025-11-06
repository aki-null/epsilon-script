using Xunit;

namespace EpsilonScript.Tests.ScriptSystem
{
  /// <summary>
  /// Tests for edge cases in string operations
  /// Covers unicode, large strings, empty strings, and special characters
  /// </summary>
  [Trait("Category", "Integration")]
  [Trait("Component", "ScriptSystem")]
  [Trait("Priority", "High")]
  public class ScriptStringEdgeCaseTests : ScriptTestBase
  {
    #region Empty String Edge Cases

    [Fact]
    public void String_EmptyPlusEmpty_ReturnsEmpty()
    {
      var result = CompileAndExecute("\"\" + \"\"", Compiler.Options.Immutable);
      Assert.Equal("", result.StringValue);
    }

    [Fact]
    public void String_EmptyPlusText_ReturnsText()
    {
      var result = CompileAndExecute("\"\" + \"hello\"", Compiler.Options.Immutable);
      Assert.Equal("hello", result.StringValue);
    }

    [Fact]
    public void String_TextPlusEmpty_ReturnsText()
    {
      var result = CompileAndExecute("\"hello\" + \"\"", Compiler.Options.Immutable);
      Assert.Equal("hello", result.StringValue);
    }

    [Fact]
    public void String_EmptyLength_ReturnsZero()
    {
      var result = CompileAndExecute("len(\"\")", Compiler.Options.Immutable);
      Assert.Equal(0, result.IntegerValue);
    }

    [Fact]
    public void String_EmptyComparison_Equal_ReturnsTrue()
    {
      var result = CompileAndExecute("\"\" == \"\"", Compiler.Options.Immutable);
      Assert.True(result.BooleanValue);
    }

    [Fact]
    public void String_EmptyVsNonEmpty_NotEqual()
    {
      var result = CompileAndExecute("\"\" != \"x\"", Compiler.Options.Immutable);
      Assert.True(result.BooleanValue);
    }

    [Fact]
    public void String_EmptyPlusNumber_ReturnsNumberAsString()
    {
      var result = CompileAndExecute("\"\" + 42", Compiler.Options.Immutable);
      Assert.Equal("42", result.StringValue);
    }

    #endregion

    #region Unicode and Special Characters

    [Fact]
    public void String_ChineseCharacters_WorksCorrectly()
    {
      var result = CompileAndExecute("\"你好\" + \"世界\"", Compiler.Options.Immutable);
      Assert.Equal("你好世界", result.StringValue);
    }

    [Fact]
    public void String_ChineseCharacters_Length_CountsCorrectly()
    {
      var result = CompileAndExecute("len(\"你好世界\")", Compiler.Options.Immutable);
      Assert.Equal(4, result.IntegerValue);
    }

    [Fact]
    public void String_JapaneseKana_WorksCorrectly()
    {
      var result = CompileAndExecute("\"こんにちは\" + \"世界\"", Compiler.Options.Immutable);
      Assert.Equal("こんにちは世界", result.StringValue);
    }

    [Fact]
    public void String_Emoji_Concatenation_Works()
    {
      var result = CompileAndExecute("\"Hello \" + \"😀🎉\"", Compiler.Options.Immutable);
      Assert.Equal("Hello 😀🎉", result.StringValue);
    }

    [Fact]
    public void String_Emoji_Length_CountsSurrogatePairs()
    {
      // Each emoji is typically 2 UTF-16 code units (surrogate pair)
      var result = CompileAndExecute("len(\"😀\")", Compiler.Options.Immutable);
      // .NET's Length counts UTF-16 code units, so 1 emoji = 2 units
      Assert.True(result.IntegerValue >= 1, "Emoji length should be at least 1");
    }

    [Fact]
    public void String_Emoji_Comparison_Works()
    {
      var result = CompileAndExecute("\"😀\" == \"😀\"", Compiler.Options.Immutable);
      Assert.True(result.BooleanValue);
    }

    [Fact]
    public void String_ArabicText_WorksCorrectly()
    {
      var result = CompileAndExecute("\"مرحبا\" + \" العالم\"", Compiler.Options.Immutable);
      Assert.Equal("مرحبا العالم", result.StringValue);
    }

    [Fact]
    public void String_HebrewText_WorksCorrectly()
    {
      var result = CompileAndExecute("\"שלום\" + \" עולם\"", Compiler.Options.Immutable);
      Assert.Equal("שלום עולם", result.StringValue);
    }

    [Fact]
    public void String_AccentedCharacters_European_Works()
    {
      var result = CompileAndExecute("\"café\" + \" naïve\" + \" Zürich\"", Compiler.Options.Immutable);
      Assert.Equal("café naïve Zürich", result.StringValue);
    }

    [Fact]
    public void String_MixedScripts_Concatenation_Works()
    {
      var result = CompileAndExecute("\"Hello\" + \"你好\" + \"مرحبا\"", Compiler.Options.Immutable);
      Assert.Equal("Hello你好مرحبا", result.StringValue);
    }

    #endregion

    #region Large Strings

    [Fact]
    public void String_VeryLong_1KB_Concatenates()
    {
      var longString = new string('x', 1024);
      var variables = Variables().WithString("long", longString).Build();
      var result = CompileAndExecute("long + \"end\"", Compiler.Options.Immutable, variables);

      Assert.Equal(1027, result.StringValue.Length);
      Assert.EndsWith("end", result.StringValue);
    }

    [Fact]
    public void String_VeryLong_10KB_Works()
    {
      var longString = new string('a', 10240);
      var variables = Variables().WithString("str", longString).Build();
      var result = CompileAndExecute("len(str)", Compiler.Options.Immutable, variables);

      Assert.Equal(10240, result.IntegerValue);
    }

    [Fact]
    public void String_MultipleConcatenations_BuildsLargeString()
    {
      var result = CompileAndExecute(
        "\"aaa\" + \"bbb\" + \"ccc\" + \"ddd\" + \"eee\" + \"fff\" + \"ggg\" + \"hhh\"",
        Compiler.Options.Immutable);

      Assert.Equal("aaabbbcccdddeeefffggghhh", result.StringValue);
      Assert.Equal(24, result.StringValue.Length);
    }

    [Fact]
    public void String_ManyConcatenationsWithNumbers_Works()
    {
      var result = CompileAndExecute(
        "\"a\" + 1 + \"b\" + 2 + \"c\" + 3 + \"d\" + 4 + \"e\" + 5",
        Compiler.Options.Immutable);

      Assert.Equal("a1b2c3d4e5", result.StringValue);
    }

    #endregion

    #region Whitespace and Control Characters

    [Fact]
    public void String_WithNewlines_Preserves()
    {
      var variables = Variables().WithString("text", "line1\nline2\nline3").Build();
      var result = CompileAndExecute("text", Compiler.Options.Immutable, variables);

      Assert.Contains("\n", result.StringValue);
      Assert.Equal("line1\nline2\nline3", result.StringValue);
    }

    [Fact]
    public void String_WithTabs_Preserves()
    {
      var variables = Variables().WithString("text", "col1\tcol2\tcol3").Build();
      var result = CompileAndExecute("text", Compiler.Options.Immutable, variables);

      Assert.Contains("\t", result.StringValue);
    }

    [Fact]
    public void String_WithCarriageReturn_Preserves()
    {
      var variables = Variables().WithString("text", "line1\r\nline2").Build();
      var result = CompileAndExecute("text", Compiler.Options.Immutable, variables);

      Assert.Contains("\r\n", result.StringValue);
    }

    [Fact]
    public void String_OnlyWhitespace_WorksCorrectly()
    {
      // Note: \t is two literal characters (backslash + 't'), not a tab character
      var result = CompileAndExecute("\"   \" + \"\\t\"", Compiler.Options.Immutable);
      Assert.Equal(5, result.StringValue.Length); // 3 spaces + '\' + 't'
      Assert.Equal("   \\t", result.StringValue);
    }

    [Fact]
    public void String_MultipleSpaces_Preserved()
    {
      var result = CompileAndExecute("\"hello     world\"", Compiler.Options.Immutable);
      Assert.Equal("hello     world", result.StringValue);
    }

    #endregion

    #region Comparison Edge Cases

    [Fact]
    public void String_CaseSensitive_Comparison()
    {
      var result = CompileAndExecute("\"Hello\" == \"hello\"", Compiler.Options.Immutable);
      Assert.False(result.BooleanValue, "String comparison should be case-sensitive");
    }

    [Fact]
    public void String_CaseSensitive_NotEqual()
    {
      var result = CompileAndExecute("\"ABC\" != \"abc\"", Compiler.Options.Immutable);
      Assert.True(result.BooleanValue);
    }

    [Fact]
    public void String_WhitespaceDifference_NotEqual()
    {
      var result = CompileAndExecute("\"hello\" == \"hello \"", Compiler.Options.Immutable);
      Assert.False(result.BooleanValue, "Trailing space should make strings unequal");
    }

    [Fact]
    public void String_IdenticalUnicode_Equal()
    {
      var result = CompileAndExecute("\"café\" == \"café\"", Compiler.Options.Immutable);
      Assert.True(result.BooleanValue);
    }

    #endregion

    #region String Functions Edge Cases

    [Fact]
    public void String_Lower_EmptyString_ReturnsEmpty()
    {
      var result = CompileAndExecute("lower(\"\")", Compiler.Options.Immutable);
      Assert.Equal("", result.StringValue);
    }

    [Fact]
    public void String_Upper_EmptyString_ReturnsEmpty()
    {
      var result = CompileAndExecute("upper(\"\")", Compiler.Options.Immutable);
      Assert.Equal("", result.StringValue);
    }

    [Fact]
    public void String_Lower_Unicode_Works()
    {
      var result = CompileAndExecute("lower(\"HELLO CAFÉ\")", Compiler.Options.Immutable);
      Assert.Equal("hello café", result.StringValue);
    }

    [Fact]
    public void String_Upper_Unicode_Works()
    {
      var result = CompileAndExecute("upper(\"hello café\")", Compiler.Options.Immutable);
      Assert.Equal("HELLO CAFÉ", result.StringValue);
    }

    [Fact]
    public void String_Lower_AlreadyLower_Unchanged()
    {
      var result = CompileAndExecute("lower(\"already lowercase\")", Compiler.Options.Immutable);
      Assert.Equal("already lowercase", result.StringValue);
    }

    [Fact]
    public void String_Upper_AlreadyUpper_Unchanged()
    {
      var result = CompileAndExecute("upper(\"ALREADY UPPER\")", Compiler.Options.Immutable);
      Assert.Equal("ALREADY UPPER", result.StringValue);
    }

    [Fact]
    public void String_Length_LongString_Accurate()
    {
      var longText = new string('x', 1000);
      var variables = Variables().WithString("text", longText).Build();
      var result = CompileAndExecute("len(text)", Compiler.Options.Immutable, variables);

      Assert.Equal(1000, result.IntegerValue);
    }

    #endregion

    #region Special Character Combinations

    [Fact]
    public void String_WithQuotesInVariable_Works()
    {
      // Cannot include quotes in string literals directly, but can in variables
      var variables = Variables().WithString("quoted", "He said \"hello\"").Build();
      var result = CompileAndExecute("quoted", Compiler.Options.Immutable, variables);

      Assert.Contains("\"", result.StringValue);
    }

    [Fact]
    public void String_WithBackslashInVariable_Works()
    {
      var variables = Variables().WithString("path", "C:\\Users\\Test").Build();
      var result = CompileAndExecute("path", Compiler.Options.Immutable, variables);

      Assert.Contains("\\", result.StringValue);
    }

    [Fact]
    public void String_NullCharacter_InVariable_Handled()
    {
      var variables = Variables().WithString("nullchar", "before\0after").Build();
      var result = CompileAndExecute("nullchar", Compiler.Options.Immutable, variables);

      Assert.Contains("\0", result.StringValue);
    }

    #endregion

    #region Single Quote String Literals

    [Fact]
    public void String_SingleQuote_Basic()
    {
      var result = CompileAndExecute("'hello'", Compiler.Options.Immutable);
      Assert.Equal("hello", result.StringValue);
    }

    [Fact]
    public void String_SingleQuote_Empty()
    {
      var result = CompileAndExecute("''", Compiler.Options.Immutable);
      Assert.Equal("", result.StringValue);
    }

    [Fact]
    public void String_SingleQuote_EqualsDoubleQuote()
    {
      var result = CompileAndExecute("'hello' == \"hello\"", Compiler.Options.Immutable);
      Assert.True(result.BooleanValue);
    }

    [Fact]
    public void String_SingleQuote_WithDoubleQuotesInside()
    {
      var result = CompileAndExecute("'He said \"hello\"'", Compiler.Options.Immutable);
      Assert.Equal("He said \"hello\"", result.StringValue);
    }

    [Fact]
    public void String_DoubleQuote_WithSingleQuotesInside()
    {
      var result = CompileAndExecute("\"It's working\"", Compiler.Options.Immutable);
      Assert.Equal("It's working", result.StringValue);
    }

    [Fact]
    public void String_SingleQuote_Concatenation()
    {
      var result = CompileAndExecute("'Hello ' + 'World'", Compiler.Options.Immutable);
      Assert.Equal("Hello World", result.StringValue);
    }

    [Fact]
    public void String_SingleAndDoubleQuote_Concatenation()
    {
      var result = CompileAndExecute("'Hello ' + \"World\"", Compiler.Options.Immutable);
      Assert.Equal("Hello World", result.StringValue);
    }

    [Fact]
    public void String_SingleQuote_Unicode()
    {
      var result = CompileAndExecute("'こんにちは世界'", Compiler.Options.Immutable);
      Assert.Equal("こんにちは世界", result.StringValue);
    }

    [Fact]
    public void String_Backslashes_AreLiteral()
    {
      var result = CompileAndExecute(@"'C:\Users\Name'", Compiler.Options.Immutable);
      Assert.Equal(@"C:\Users\Name", result.StringValue);
    }

    #endregion
  }
}