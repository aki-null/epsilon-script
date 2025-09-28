using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;
using EpsilonScript.Tests.TestInfrastructure.Fakes;

namespace EpsilonScript.Tests.RpnConverter
{
  [Trait("Category", "Unit")]
  [Trait("Component", "RpnConverter")]
  public class RPNConverter_Comparison : RpnConverterTestBase
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void RPNConverter_ParsesComparison_Correctly(Element[] input, Element[] expected)
    {
      AssertRpnSucceeds(input, expected);
    }

    public static IEnumerable<object[]> CorrectData
    {
      get
      {
        return new[]
        {
          // Basic comparison operations
          CreateTestCase("1 < 2", "1 2 <"),
          CreateTestCase("1 > 2", "1 2 >"),
          CreateTestCase("1 <= 2", "1 2 <="),
          CreateTestCase("1 >= 2", "1 2 >="),
          CreateTestCase("1 == 2", "1 2 =="),
          CreateTestCase("1 != 2", "1 2 !="),

          // Comparison with variables
          CreateTestCase("left < right", "left right <"),
          CreateTestCase("left == right", "left right =="),

          // Comparison with expressions
          CreateTestCase("1 + 2 < 3 + 4", "1 2 + 3 4 + <"),
          CreateTestCase("1 * 2 == 3 - 1", "1 2 * 3 1 - =="),

          // Multiple comparisons (chaining)
          CreateTestCase("1 < 2 < 3", "1 2 < 3 <"),
          CreateTestCase("1 == 2 != 3", "1 2 == 3 !="),

          // Comparisons with precedence
          CreateTestCase("1 + 2 < 3 * 4", "1 2 + 3 4 * <"),
          CreateTestCase("1 < 2 + 3", "1 2 3 + <"),
        };
      }
    }
  }
}