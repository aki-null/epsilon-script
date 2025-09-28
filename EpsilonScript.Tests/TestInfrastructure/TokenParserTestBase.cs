using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure.Fakes;

namespace EpsilonScript.Tests.TestInfrastructure
{
  public abstract class TokenParserTestBase
  {
    protected static void AssertParseSucceeds(IList<Token> tokens, IList<Element> expected)
    {
      var elementReader = new TestElementReader();
      var parser = new EpsilonScript.Parser.TokenParser(elementReader);
      foreach (var token in tokens)
      {
        parser.Push(token);
      }

      parser.End();
      Assert.True(elementReader.EndCalled, "Element reader not closed");

      var output = elementReader.Elements;
      Assert.Equal(expected.Count, output.Count);
      for (var i = 0; i < output.Count; ++i)
      {
        Assert.Equal(expected[i], output[i]);
      }
    }

    protected static object[] CreateTestCase(params Element[] elements)
    {
      var tokens = new Token[elements.Length];
      for (var i = 0; i < elements.Length; ++i)
      {
        tokens[i] = elements[i].Token;
      }

      return new object[] { tokens, elements };
    }
  }
}
