using System;
using System.Runtime.CompilerServices;

namespace EpsilonScript.Lexer
{
  /// <summary>
  /// Encapsulates reading from a buffer and tracking position.
  /// </summary>
  internal ref struct LexerCursor
  {
    private readonly ReadOnlySpan<char> _buffer;
    private LexerPosition _current;
    private LexerPosition _tokenStart;

    public LexerCursor(ReadOnlyMemory<char> buffer)
    {
      _buffer = buffer.Span;
      _current = LexerPosition.Start;
      _tokenStart = LexerPosition.Start;
    }

    /// <summary>
    /// Current position being read
    /// </summary>
    public LexerPosition Current => _current;

    /// <summary>
    /// Position where current token started
    /// </summary>
    public LexerPosition TokenStart => _tokenStart;

    /// <summary>
    /// Check if at end of buffer
    /// </summary>
    public bool IsAtEnd => _current.Offset >= _buffer.Length;

    /// <summary>
    /// Peek at current character without advancing.
    /// Returns '\0' (EOF) if at end of buffer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public char Peek()
    {
      return _current.Offset < _buffer.Length ? _buffer[_current.Offset] : '\0';
    }

    /// <summary>
    /// Advance to next character and return it.
    /// Updates line/column tracking automatically.
    /// Returns '\0' (EOF) if at end of buffer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public char Advance()
    {
      var c = Peek();

      if (c == '\0') return c;

      _current = _current.Advance(c);

      return c;
    }

    /// <summary>
    /// Mark current position as the start of the next token.
    /// Call this after consuming whitespace or after emitting a token.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MarkTokenStart()
    {
      _tokenStart = _current;
    }

    /// <summary>
    /// Get the slice of buffer from token start to current position.
    /// Use this with the original ReadOnlyMemory to extract token text.
    /// </summary>
    /// <param name="buffer">Original buffer (ReadOnlyMemory)</param>
    /// <returns>Slice containing the token text</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyMemory<char> GetTokenText(ReadOnlyMemory<char> buffer)
    {
      var length = _current.Offset - _tokenStart.Offset;
      return buffer.Slice(_tokenStart.Offset, length);
    }

    /// <summary>
    /// Get source location for error reporting (uses token start position).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SourceLocation GetTokenLocation()
    {
      return new SourceLocation(_tokenStart.Line, _tokenStart.Column);
    }

    /// <summary>
    /// Check if next character matches expected (lookahead without advancing).
    /// </summary>
    /// <param name="expected">Character to check for</param>
    /// <returns>True if next character matches</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Check(char expected)
    {
      return Peek() == expected;
    }

    /// <summary>
    /// If next character matches, advance and return true.
    /// Otherwise return false without advancing.
    /// </summary>
    /// <param name="expected">Character to accept</param>
    /// <returns>True if character was matched and consumed</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Accept(char expected)
    {
      if (Check(expected))
      {
        Advance();
        return true;
      }

      return false;
    }

    /// <summary>
    /// Get current token as span (from token start to current position).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<char> GetTokenSpan()
    {
      var length = _current.Offset - _tokenStart.Offset;
      return _buffer.Slice(_tokenStart.Offset, length);
    }
  }
}