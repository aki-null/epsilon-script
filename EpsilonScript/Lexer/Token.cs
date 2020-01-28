namespace EpsilonScript.Lexer
{
  public struct Token
  {
    public readonly string Text;
    public readonly TokenType Type;
    public readonly int LineNumber;

    public Token(string text, TokenType type, int lineNumber = -1)
    {
      Text = text;
      Type = type;
      LineNumber = lineNumber;
    }

    public override string ToString()
    {
      return LineNumber < 0 ? $"{Type} ({Text})" : $"{Type} ({Text}) at line {LineNumber}";
    }

    public static bool operator ==(Token lhs, Token rhs)
    {
      return lhs.Text.Equals(rhs.Text) && lhs.Type == rhs.Type;
    }

    public static bool operator !=(Token lhs, Token rhs)
    {
      return !(lhs == rhs);
    }

    public override bool Equals(object obj)
    {
      if (!(obj is Token))
      {
        return false;
      }

      var rhs = (Token) obj;
      return Text.Equals(rhs.Text) && Type == rhs.Type;
    }

    public override int GetHashCode()
    {
      var hash = 1009;
      hash = hash * 9176 + Text.GetHashCode();
      hash = hash * 9176 + ((int) Type).GetHashCode();
      return hash;
    }
  }
}