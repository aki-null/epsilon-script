using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.RpnConverter
{
  [Trait("Category", "Unit")]
  [Trait("Component", "RpnConverter")]
  public class RPNConverter_BooleanOperator : RpnConverterTestBase
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    internal void RPNConverter_ParsesBooleanOperator_Correctly(Element[] input, Element[] expected)
    {
      AssertRpnSucceeds(input, expected);
    }

    public static IEnumerable<object[]> CorrectData
    {
      get
      {
        return new[]
        {
          // Basic boolean operations
          CreateTestCase("true || false", "true false ||"),
          CreateTestCase("true && false", "true false &&"),

          // Boolean operator precedence (&& has higher precedence than ||)
          CreateTestCase("true && false || true", "true false && true ||"),
          CreateTestCase("true || false && true", "true false true && ||"),

          // Multiple operations of same precedence
          CreateTestCase("true && false && true", "true false && true &&"),
          CreateTestCase("true || false || true", "true false || true ||"),

          // Boolean operations with variables
          CreateTestCase("left && right", "left right &&"),
          CreateTestCase("left || right", "left right ||"),

          // Mixed with comparisons
          CreateTestCase("1 < 2 && 3 > 4", "1 2 < 3 4 > &&"),
          CreateTestCase("1 == 2 || 3 != 4", "1 2 == 3 4 != ||"),

          // Parentheses override precedence
          CreateTestCase("(true || false) && true", "true false || true &&"),
          CreateTestCase("true || (false && true)", "true false true && ||"),
        };
      }
    }
  }
}