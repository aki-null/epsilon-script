using System;

namespace EpsilonScript.Intermediate
{
  internal readonly struct Token
  {
    public readonly ReadOnlyMemory<char> Text;
    public readonly TokenType Type;
    public readonly SourceLocation Location;

    public Token(ReadOnlyMemory<char> text, TokenType type, SourceLocation location)
    {
      Text = text;
      Type = type;
      Location = location;
    }

    public Token(ReadOnlyMemory<char> text, TokenType type)
      : this(text, type, SourceLocation.Unknown)
    {
    }

    // For unit tests and backwards compatibility - string overload
    public Token(string text, TokenType type, int lineNumber = -1, int characterIndex = -1)
      : this(text.AsMemory(), type, new SourceLocation(lineNumber, characterIndex))
    {
    }

    public override string ToString()
    {
      if (!Location.IsValid)
        return $"{Type} ({Text.ToString()})";

      return $"{Type} ({Text.ToString()}) at {Location}";
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