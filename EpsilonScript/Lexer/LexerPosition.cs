namespace EpsilonScript.Lexer
{
  /// <summary>
  /// Immutable snapshot of a position in the lexer buffer.
  /// Represents a single point with offset, line, and column.
  /// Used internally by the lexer to track cursor position during scanning.
  /// </summary>
  internal readonly struct LexerPosition
  {
    /// <summary>
    /// Absolute character position in buffer (0-indexed)
    /// </summary>
    public readonly int Offset;

    /// <summary>
    /// Line number (0-indexed)
    /// </summary>
    public readonly int Line;

    /// <summary>
    /// Column within line (0-indexed)
    /// </summary>
    public readonly int Column;

    private LexerPosition(int offset, int line, int column)
    {
      Offset = offset;
      Line = line;
      Column = column;
    }

    /// <summary>
    /// Initial position at start of file (0, 0, 0)
    /// </summary>
    public static LexerPosition Start => new LexerPosition(0, 0, 0);

    /// <summary>
    /// Create a new position after advancing over a character.
    /// Returns a new instance (immutable).
    /// </summary>
    /// <param name="c">Character being advanced over</param>
    /// <returns>New position after the character</returns>
    public LexerPosition Advance(char c)
    {
      return c == '\n'
        ? new LexerPosition(Offset + 1, Line + 1, 0) // New line
        : new LexerPosition(Offset + 1, Line, Column + 1); // Same line
    }

    /// <summary>
    /// Equality comparison for testing purposes.
    /// </summary>
    public bool Equals(LexerPosition other)
    {
      return Offset == other.Offset && Line == other.Line && Column == other.Column;
    }

    public override string ToString()
    {
      return $"Offset={Offset}, Line={Line}, Column={Column}";
    }
  }
}