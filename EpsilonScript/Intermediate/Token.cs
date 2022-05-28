using System;

namespace EpsilonScript.Intermediate
{
  public readonly struct Token
  {
    public readonly ReadOnlyMemory<char> Text;
    public readonly TokenType Type;
    public readonly int LineNumber;

    public Token(ReadOnlyMemory<char> text, TokenType type, int lineNumber = -1)
    {
      Text = text;
      Type = type;
      LineNumber = lineNumber;
    }

    // For unit tests
    public Token(string text, TokenType type, int lineNumber = -1) : this(text.AsMemory(), type, lineNumber)
    {
    }

    public override string ToString()
    {
      return LineNumber < 0 ? $"{Type} ({Text.ToString()})" : $"{Type} ({Text.ToString()}) at line {LineNumber}";
    }

    public static bool operator ==(Token lhs, Token rhs)
    {
      return lhs.Text.Span == rhs.Text.Span && lhs.Type == rhs.Type;
    }

    public static bool operator !=(Token lhs, Token rhs)
    {
      return !(lhs == rhs);
    }

    public override bool Equals(object obj)
    {
      if (!(obj is Token rhs))
      {
        return false;
      }

      return Text.Span == rhs.Text.Span && Type == rhs.Type;
    }

    public override int GetHashCode()
    {
      var hash = 1009;
      hash = hash * 9176 + Text.GetHashCode();
      hash = hash * 9176 + ((int)Type).GetHashCode();
      return hash;
    }
  }
}