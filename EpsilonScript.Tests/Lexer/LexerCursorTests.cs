using System;
using EpsilonScript.Lexer;
using Xunit;

namespace EpsilonScript.Tests.Lexer
{
  [Trait("Category", "Unit")]
  [Trait("Component", "LexerCursor")]
  public class LexerCursorTests
  {
    [Fact]
    public void Cursor_InitialState_IsAtStart()
    {
      var cursor = new LexerCursor("test".AsMemory());

      Assert.Equal(0, cursor.Current.Offset);
      Assert.Equal(0, cursor.Current.Line);
      Assert.Equal(0, cursor.Current.Column);
      Assert.Equal(0, cursor.TokenStart.Offset);
      Assert.False(cursor.IsAtEnd);
    }

    [Fact]
    public void Peek_ReturnsCurrentCharacter_WithoutAdvancing()
    {
      var cursor = new LexerCursor("abc".AsMemory());

      Assert.Equal('a', cursor.Peek());
      Assert.Equal('a', cursor.Peek()); // Peek again - should be same
      Assert.Equal(0, cursor.Current.Offset);
    }

    [Fact]
    public void Peek_AtEnd_ReturnsNull()
    {
      var cursor = new LexerCursor("".AsMemory());

      Assert.Equal('\0', cursor.Peek());
      Assert.True(cursor.IsAtEnd);
    }

    [Fact]
    public void Advance_SingleLine_UpdatesOffsetAndColumn()
    {
      var cursor = new LexerCursor("abc".AsMemory());

      Assert.Equal('a', cursor.Advance());
      Assert.Equal(1, cursor.Current.Offset);
      Assert.Equal(0, cursor.Current.Line);
      Assert.Equal(1, cursor.Current.Column);

      Assert.Equal('b', cursor.Advance());
      Assert.Equal(2, cursor.Current.Offset);
      Assert.Equal(0, cursor.Current.Line);
      Assert.Equal(2, cursor.Current.Column);
    }

    [Fact]
    public void Advance_OverNewline_IncrementsLineAndResetsColumn()
    {
      var cursor = new LexerCursor("a\nb".AsMemory());

      cursor.Advance(); // 'a' - line 0, col 1
      Assert.Equal(0, cursor.Current.Line);
      Assert.Equal(1, cursor.Current.Column);

      cursor.Advance(); // '\n' - line 1, col 0
      Assert.Equal(1, cursor.Current.Line);
      Assert.Equal(0, cursor.Current.Column);

      cursor.Advance(); // 'b' - line 1, col 1
      Assert.Equal(1, cursor.Current.Line);
      Assert.Equal(1, cursor.Current.Column);
    }

    [Fact]
    public void Advance_AtEnd_ReturnsNullAndDoesNotAdvance()
    {
      var cursor = new LexerCursor("a".AsMemory());

      cursor.Advance(); // 'a'
      Assert.Equal(1, cursor.Current.Offset);

      var result = cursor.Advance(); // EOF
      Assert.Equal('\0', result);
      Assert.Equal(1, cursor.Current.Offset); // Should not advance beyond buffer
    }

    [Fact]
    public void MarkTokenStart_UpdatesTokenStartPosition()
    {
      var cursor = new LexerCursor("abc".AsMemory());

      Assert.Equal(0, cursor.TokenStart.Offset);

      cursor.Advance(); // 'a'
      cursor.Advance(); // 'b'
      cursor.MarkTokenStart();

      Assert.Equal(2, cursor.TokenStart.Offset);
      Assert.Equal(0, cursor.TokenStart.Line);
      Assert.Equal(2, cursor.TokenStart.Column);
    }

    [Fact]
    public void GetTokenText_ReturnsSliceFromTokenStartToCurrent()
    {
      var buffer = "hello world".AsMemory();
      var cursor = new LexerCursor(buffer);

      cursor.Advance(); // 'h'
      cursor.Advance(); // 'e'
      cursor.Advance(); // 'l'
      cursor.Advance(); // 'l'
      cursor.Advance(); // 'o'

      var tokenText = cursor.GetTokenText(buffer);
      Assert.Equal("hello", tokenText.ToString());
    }

    [Fact]
    public void GetTokenLocation_ReturnsTokenStartLocation()
    {
      var cursor = new LexerCursor("a\nbc".AsMemory());

      cursor.Advance(); // 'a'
      cursor.Advance(); // '\n'
      cursor.MarkTokenStart(); // Start token at line 1, col 0
      cursor.Advance(); // 'b'
      cursor.Advance(); // 'c'

      var location = cursor.GetTokenLocation();
      Assert.Equal(1, location.LineNumber); // Token started on line 1
      Assert.Equal(0, location.CharacterIndex);
    }

    [Fact]
    public void Check_ReturnsTrueIfNextMatchesWithoutAdvancing()
    {
      var cursor = new LexerCursor("abc".AsMemory());

      Assert.True(cursor.Check('a'));
      Assert.Equal(0, cursor.Current.Offset); // Should not advance

      Assert.False(cursor.Check('b'));
      Assert.Equal(0, cursor.Current.Offset); // Should not advance
    }

    [Fact]
    public void Accept_MatchingCharacter_AdvancesAndReturnsTrue()
    {
      var cursor = new LexerCursor("abc".AsMemory());

      Assert.True(cursor.Accept('a'));
      Assert.Equal(1, cursor.Current.Offset);
      Assert.Equal('b', cursor.Peek());
    }

    [Fact]
    public void Accept_NonMatchingCharacter_DoesNotAdvanceReturnsFalse()
    {
      var cursor = new LexerCursor("abc".AsMemory());

      Assert.False(cursor.Accept('x'));
      Assert.Equal(0, cursor.Current.Offset);
      Assert.Equal('a', cursor.Peek());
    }

    [Fact]
    public void GetTokenSpan_ReturnsSpanFromTokenStartToCurrent()
    {
      var cursor = new LexerCursor("hello world".AsMemory());

      cursor.Advance(); // 'h'
      cursor.Advance(); // 'e'
      cursor.Advance(); // 'l'
      cursor.Advance(); // 'l'
      cursor.Advance(); // 'o'

      var tokenSpan = cursor.GetTokenSpan();
      Assert.True(tokenSpan.SequenceEqual("hello".AsSpan()));
    }

    [Fact]
    public void MultipleLines_PositionTrackingIsCorrect()
    {
      var cursor = new LexerCursor("line1\nline2\nline3".AsMemory());

      // Consume "line1\n"
      for (var i = 0; i < 6; i++) cursor.Advance();
      Assert.Equal(1, cursor.Current.Line);
      Assert.Equal(0, cursor.Current.Column);

      // Consume "line2\n"
      for (var i = 0; i < 6; i++) cursor.Advance();
      Assert.Equal(2, cursor.Current.Line);
      Assert.Equal(0, cursor.Current.Column);

      // Consume "li"
      cursor.Advance();
      cursor.Advance();
      Assert.Equal(2, cursor.Current.Line);
      Assert.Equal(2, cursor.Current.Column);
    }
  }
}