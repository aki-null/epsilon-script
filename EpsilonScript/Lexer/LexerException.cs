using System;

namespace EpsilonScript.Lexer
{
  [Serializable]
  public class LexerException : Exception
  {
    public int LineNumber { get; private set; }

    public LexerException(int lineNumber)
    {
      LineNumber = lineNumber;
    }

    public LexerException(int lineNumber, string message)
      : base(message)
    {
      LineNumber = lineNumber;
    }

    public LexerException(int lineNumber, string message, Exception inner)
      : base(message, inner)
    {
      LineNumber = lineNumber;
    }

    public override string ToString()
    {
      return $"Line {LineNumber}: {Message}";
    }
  }
}