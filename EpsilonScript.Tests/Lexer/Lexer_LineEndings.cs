using System;
using EpsilonScript.Lexer;
using EpsilonScript.Tests.TestInfrastructure;
using EpsilonScript.Tests.TestInfrastructure.Fakes;
using Xunit;

namespace EpsilonScript.Tests.Lexer
{
  [Trait("Category", "Unit")]
  [Trait("Component", "Lexer")]
  public class Lexer_LineEndings : LexerTestBase
  {
    private TestTokenReader Tokenize(string source)
    {
      var lexer = new EpsilonScript.Lexer.Lexer();
      var tokenReader = new TestTokenReader();
      lexer.Execute(source.AsMemory(), tokenReader);
      return tokenReader;
    }

    [Fact]
    public void Lexer_UnixLineEndings_TracksLineNumbers()
    {
      // Unix line endings: \n
      var script = "foo\nbar\nbaz";

      var tokenReader = Tokenize(script);
      var tokens = tokenReader.Tokens;

      // All identifiers should be on different lines
      Assert.Equal(0, tokens[0].Location.LineNumber); // foo on line 0
      Assert.Equal(1, tokens[1].Location.LineNumber); // bar on line 1
      Assert.Equal(2, tokens[2].Location.LineNumber); // baz on line 2
    }

    [Fact]
    public void Lexer_WindowsLineEndings_TracksLineNumbers()
    {
      // Windows line endings: \r\n
      var script = "foo\r\nbar\r\nbaz";

      var tokenReader = Tokenize(script);
      var tokens = tokenReader.Tokens;

      // All identifiers should be on different lines
      // \r is whitespace, \n increments line number
      Assert.Equal(0, tokens[0].Location.LineNumber); // foo on line 0
      Assert.Equal(1, tokens[1].Location.LineNumber); // bar on line 1
      Assert.Equal(2, tokens[2].Location.LineNumber); // baz on line 2
    }

    [Fact]
    public void Lexer_OldMacLineEndings_LimitedSupport()
    {
      // Old Mac line endings: \r only
      var script = "foo\rbar\rbaz";

      var tokenReader = Tokenize(script);
      var tokens = tokenReader.Tokens;

      // KNOWN LIMITATION: \r alone doesn't increment line numbers
      // All tokens appear on the same line
      Assert.Equal(0, tokens[0].Location.LineNumber); // foo on line 0
      Assert.Equal(0, tokens[1].Location.LineNumber); // bar on line 0 (not 1)
      Assert.Equal(0, tokens[2].Location.LineNumber); // baz on line 0 (not 2)

      // This is acceptable because:
      // 1. Old Mac (\r-only) line endings are extremely rare in modern code
      // 2. Modern Mac uses Unix (\n) line endings
      // 3. The lexer still parses correctly, just line numbers are off
    }

    [Fact]
    public void Lexer_MixedLineEndings_HandlesCorrectly()
    {
      // Mixed: \n and \r\n
      var script = "foo\nbar\r\nbaz";

      var tokenReader = Tokenize(script);
      var tokens = tokenReader.Tokens;

      Assert.Equal(0, tokens[0].Location.LineNumber); // foo on line 0
      Assert.Equal(1, tokens[1].Location.LineNumber); // bar on line 1
      Assert.Equal(2, tokens[2].Location.LineNumber); // baz on line 2
    }

    [Fact]
    public void Lexer_MultipleConsecutiveNewlines_TracksCorrectly()
    {
      var script = "foo\n\n\nbar";

      var tokenReader = Tokenize(script);
      var tokens = tokenReader.Tokens;

      Assert.Equal(0, tokens[0].Location.LineNumber); // foo on line 0
      Assert.Equal(3, tokens[1].Location.LineNumber); // bar on line 3 (after 3 newlines)
    }

    [Fact]
    public void Lexer_ErrorAfterNewline_ReportsCorrectLine()
    {
      // Invalid character after newline
      var script = "foo\n@";

      var ex = Assert.Throws<LexerException>(() => Tokenize(script));

      // Error should be reported on line 1 (second line, 0-indexed)
      Assert.Equal(1, ex.LineNumber);
      Assert.Contains("@", ex.Message);
    }
  }
}