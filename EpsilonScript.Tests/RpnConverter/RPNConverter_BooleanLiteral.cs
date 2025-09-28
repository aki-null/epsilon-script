using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.RpnConverter
{
  public class RPNConverter_BooleanLiteral : RpnConverterTestBase
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void RPNConverter_ParsesBooleanLiteral_Correctly(Element[] input, Element[] expected)
    {
      AssertRpnSucceeds(input, expected);
    }

    public static IEnumerable<object[]> CorrectData
    {
      get
      {
        return new[]
        {
          // Boolean literals pass through unchanged
          CreateTestCase("true", "true"),
          CreateTestCase("false", "false"),
          CreateTestCase("true false", "true false"),
        };
      }
    }
  }
}