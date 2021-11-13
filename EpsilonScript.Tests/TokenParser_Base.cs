using System.Collections.Generic;
using EpsilonScript.Intermediate;
using EpsilonScript.Parser;
using Xunit;

namespace EpsilonScript.Tests
{
  public class TokenParser_Base
  {
    protected static void Succeeds(IList<Token> input, IList<Element> expected)
    {
      var elementReader = new TestElementReader();
      var tokenParser = new TokenParser(elementReader);
      foreach (var token in input)
      {
        tokenParser.Push(token);
      }

      tokenParser.End();
      var output = elementReader.Elements;
      Assert.Equal(output.Count, expected.Count);

      for (var i = 0; i < output.Count; ++i)
      {
        Assert.Equal(output[i], expected[i]);
      }

      Assert.True(elementReader.EndCalled, "Element reader not closed");
    }

    protected static object[] CreateTestData(params Element[] elements)
    {
      var tokens = new Token[elements.Length];
      for (var i = 0; i < elements.Length; ++i)
      {
        tokens[i] = elements[i].Token;
      }

      return
        new object[]
        {
          tokens,
          elements
        };
    }
  }
}