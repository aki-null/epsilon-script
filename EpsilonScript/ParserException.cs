using System;
using EpsilonScript.Intermediate;

namespace EpsilonScript
{
  [Serializable]
  public class ParserException : Exception
  {
    public SourceLocation Location { get; }

    private static string FormatMessage(SourceLocation location, string message)
    {
      if (!location.IsValid)
        return string.IsNullOrEmpty(message) ? "Unknown error" : message;

      var locationStr = location.ToString();
      return string.IsNullOrEmpty(message) ? $"{locationStr}: Unknown error" : $"{locationStr}: {message}";
    }

    internal ParserException(SourceLocation location) : base(FormatMessage(location, string.Empty))
    {
      Location = location;
    }

    internal ParserException(SourceLocation location, string message)
      : base(FormatMessage(location, message))
    {
      Location = location;
    }

    internal ParserException(in Token token, string message) : this(token.Location, message)
    {
    }
  }
}