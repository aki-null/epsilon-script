using System.Collections.Generic;
using EpsilonScript.Parser;
using Xunit;

namespace EpsilonScript.Tests
{
  public abstract class RPNConverter_Base
  {
    protected static void Succeeds(IList<Element> input, IList<Element> expected)
    {
      var rpnConverter = new RPNConverter();
      rpnConverter.Convert(new List<Element>(input));

      var output = rpnConverter.Rpn;
      Assert.Equal(output.Count, expected.Count);

      for (var i = 0; i < output.Count; ++i)
      {
        Assert.Equal(output[i], expected[i]);
      }
    }

    protected static void Fails(IList<Element> input)
    {
      var rpnConverter = new RPNConverter();
      Assert.Throws<ParserException>(() => { rpnConverter.Convert(new List<Element>(input)); });
    }
  }
}