using System;
using EpsilonScript.Intermediate;

namespace EpsilonScript
{
  [Serializable]
  internal class ParserException : Exception
  {
    private static string FormatMessage(in Token token, string message)
    {
      return string.IsNullOrEmpty(message) ? $"{token.ToString()}: Unknown error" : $"{token.ToString()}: {message}";
    }

    public ParserException(in Token token) : base(FormatMessage(token, ""))
    {
    }

    public ParserException(in Token token, string message)
      : base(FormatMessage(token, message))
    {
    }

    public ParserException(in Token token, string message, Exception inner)
      : base(FormatMessage(token, message), inner)
    {
    }
  }
}