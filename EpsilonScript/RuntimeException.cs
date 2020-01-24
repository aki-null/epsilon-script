using System;

namespace EpsilonScript.Parser
{
  [Serializable]
  public class RuntimeException : Exception
  {
    public RuntimeException()
    {
    }

    public RuntimeException(string message)
      : base(message)
    {
    }

    public RuntimeException(string message, Exception inner)
      : base(message, inner)
    {
    }
  }
}