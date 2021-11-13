using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;

namespace EpsilonScript.Tests
{
  public class TestTokenReader : ITokenReader
  {
    public bool EndCalled { get; private set; }

    public List<Token> Tokens { get; } = new List<Token>();

    public void Push(Token token)
    {
      Tokens.Add(token);
    }

    public void End()
    {
      Assert.False(EndCalled);
      EndCalled = true;
    }
  }
}