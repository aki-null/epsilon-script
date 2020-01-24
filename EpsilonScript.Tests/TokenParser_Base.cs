using System.Collections.Generic;
using EpsilonScript.Lexer;
using EpsilonScript.Parser;
using Xunit;

namespace EpsilonScript.Tests
{
  public class TokenParser_Base
  {
    protected static void Succeeds(IList<Token> input, IList<Element> expected)
    {
      var tokenParser = new TokenParser();
      tokenParser.Parse(input);
      var output = tokenParser.Elements;
      Assert.Equal(output.Count, expected.Count);

      for (var i = 0; i < output.Count; ++i)
      {
        Assert.Equal(output[i], expected[i]);
      }
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