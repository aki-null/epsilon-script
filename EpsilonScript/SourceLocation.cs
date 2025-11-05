namespace EpsilonScript
{
  /// <summary>
  /// Represents a location in source code for error reporting.
  /// Both line number and character index are stored as 0-indexed internally,
  /// When displayed to users, both are converted to 1-indexed for human readability.
  /// </summary>
  public readonly struct SourceLocation
  {
    /// <summary>
    /// 0-indexed line number (first line is 0)
    /// </summary>
    public readonly int LineNumber;

    /// <summary>
    /// 0-indexed character position within the line (first character is 0)
    /// </summary>
    public readonly int CharacterIndex;

    public SourceLocation(int lineNumber, int characterIndex)
    {
      LineNumber = lineNumber;
      CharacterIndex = characterIndex;
    }

    public static SourceLocation Unknown => new SourceLocation(-1, -1);

    public bool IsValid => LineNumber >= 0;

    public override string ToString()
    {
      if (LineNumber < 0)
        return "unknown location";

      if (CharacterIndex >= 0)
        return $"line {LineNumber + 1}, column {CharacterIndex + 1}";

      return $"line {LineNumber + 1}";
    }
  }
}