using System;

namespace EpsilonScript
{
  [Serializable]
  public class RuntimeException : Exception
  {
    public SourceLocation Location { get; }

    private static string FormatMessage(string message, SourceLocation location)
    {
      if (!location.IsValid)
        return message;

      return $"Runtime error at {location}: {message}";
    }

    public RuntimeException()
    {
      Location = SourceLocation.Unknown;
    }

    public RuntimeException(string message) : base(message)
    {
      Location = SourceLocation.Unknown;
    }

    internal RuntimeException(string message, SourceLocation location)
      : base(FormatMessage(message, location))
    {
      Location = location;
    }
  }
}