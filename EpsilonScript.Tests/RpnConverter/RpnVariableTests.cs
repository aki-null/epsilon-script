using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.RpnConverter
{
  public class RpnVariableTests : RpnConverterTestBase
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    internal void RpnConverter_VariableReferences_ConvertCorrectly(Element[] input, Element[] expected)
    {
      AssertRpnSucceeds(input, expected);
    }

    public static IEnumerable<object[]> CorrectData
    {
      get
      {
        return new[]
        {
          // Variables pass through unchanged
          CreateTestCase("var", "var"),
          CreateTestCase("variable", "variable"),
          CreateTestCase("myVar", "myVar"),

          // Multiple variables
          CreateTestCase("var1 var2", "var1 var2"),
          CreateTestCase("left right", "left right"),
        };
      }
    }
  }
}