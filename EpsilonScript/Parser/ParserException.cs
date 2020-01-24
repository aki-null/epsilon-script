using System;
using EpsilonScript.Lexer;

namespace EpsilonScript.Parser
{
  [Serializable]
  public class ParserException : Exception
  {
    public int LineNumber { get; private set; }
    public int Position { get; private set; }

    public ParserException(in Token token)
    {
      LineNumber = token.LineNumber;
      Position = token.Position;
    }

    public ParserException(in Token token, string message)
      : base(message)
    {
      LineNumber = token.LineNumber;
      Position = token.Position;
    }

    public ParserException(in Token token, string message, Exception inner)
      : base(message, inner)
    {
      LineNumber = token.LineNumber;
      Position = token.Position;
    }

    public override string ToString()
    {
      return $"{LineNumber}:{Position}: {Message}";
    }
  }
}