using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;

namespace EpsilonScript.Tests.TestInfrastructure.Fakes
{
  internal class TestElementReader : IElementReader
  {
    public bool EndCalled { get; private set; }

    public List<Element> Elements { get; } = new List<Element>();

    public void Push(Element element)
    {
      Elements.Add(element);
    }

    public void End()
    {
      Assert.False(EndCalled);
      EndCalled = true;
    }
  }
}