using System.Collections.Generic;
using EpsilonScript.Intermediate;
using EpsilonScript.Parser;
using Xunit;

namespace EpsilonScript.Tests
{
  public abstract class RPNConverter_Base
  {
    internal static void Succeeds(IList<Element> input, IList<Element> expected)
    {
      var elementReader = new TestElementReader();
      var rpnConverter = new RpnConverter(elementReader);
      foreach (var element in input)
      {
        rpnConverter.Push(element);
      }

      rpnConverter.End();
      Assert.True(elementReader.EndCalled, "Element reader not closed");

      var output = elementReader.Elements;
      Assert.Equal(output.Count, expected.Count);

      for (var i = 0; i < output.Count; ++i)
      {
        Assert.Equal(output[i], expected[i]);
      }
    }

    internal static void Fails(IList<Element> input)
    {
      var elementReader = new TestElementReader();
      var rpnConverter = new RpnConverter(elementReader);
      Assert.Throws<ParserException>(() =>
      {
        foreach (var element in input)
        {
          rpnConverter.Push(element);
        }

        rpnConverter.End();
      });
    }
  }
}