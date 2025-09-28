using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;
using EpsilonScript.Tests.TestInfrastructure.Fakes;

namespace EpsilonScript.Tests.RpnConverter
{
  public class RPNConverter_Negate : RpnConverterTestBase
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void RPNConverter_ParsesNegate_Correctly(Element[] input, Element[] expected)
    {
      AssertRpnSucceeds(input, expected);
    }

    public static IEnumerable<object[]> CorrectData
    {
      get
      {
        return new[]
        {
          // Basic negation
          CreateTestCase("! var", "var !"),
          CreateTestCase("! true", "true !"),
          CreateTestCase("! false", "false !"),

          // Negation with expressions
          CreateTestCase("! (1 < 2)", "1 2 < !"),
          CreateTestCase("! (left == right)", "left right == !"),

          // Multiple negations
          CreateTestCase("! ! true", "true ! !"),

          // Negation with boolean operations
          CreateTestCase("! left && right", "left ! right &&"),
          CreateTestCase("! (left && right)", "left right && !"),
        };
      }
    }
  }
}