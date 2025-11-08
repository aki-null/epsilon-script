using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.RpnConverter
{
  public class RpnBooleanLiteralTests : RpnConverterTestBase
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    internal void RpnConverter_BooleanLiterals_ConvertCorrectly(Element[] input, Element[] expected)
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