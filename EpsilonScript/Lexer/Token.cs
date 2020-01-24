namespace EpsilonScript.Lexer
{
  public struct Token
  {
    public readonly string Text;
    public readonly TokenType Type;
    public readonly int LineNumber;
    public readonly int Position;

    public Token(string text, TokenType type, int lineNumber = -1, int position = -1)
    {
      Text = text;
      Type = type;
      LineNumber = lineNumber;
      Position = position;
    }

    public override string ToString()
    {
      return LineNumber < 0 ? $"{Type}: {Text}" : $"{Type}: {Text} at {LineNumber}:{Position}";
    }

    public static bool operator ==(Token lhs, Token rhs)
    {
      return lhs.Text.Equals(rhs.Text) && lhs.Type == rhs.Type;
    }

    public static bool operator !=(Token lhs, Token rhs)
    {
      return !(lhs == rhs);
    }
  }
}