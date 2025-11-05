using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.RpnConverter
{
  [Trait("Category", "Unit")]
  [Trait("Component", "RpnConverter")]
  public class RpnMathTests : RpnConverterTestBase
  {
    [Theory]
    [MemberData(nameof(SimpleMathData))]
    internal void ParsesSimpleMath_Correctly(Element[] input, Element[] expected)
    {
      AssertRpnSucceeds(input, expected);
    }

    [Theory]
    [MemberData(nameof(PrecedenceData))]
    internal void ParsesPrecedence_Correctly(Element[] input, Element[] expected)
    {
      AssertRpnSucceeds(input, expected);
    }

    [Theory]
    [MemberData(nameof(ComplexExpressionData))]
    internal void ParsesComplexMath_Correctly(Element[] input, Element[] expected)
    {
      AssertRpnSucceeds(input, expected);
    }

    public static IEnumerable<object[]> SimpleMathData
    {
      get
      {
        return new[]
        {
          // Simple literals pass through unchanged
          CreateTestCase("1", "1"),
          CreateTestCase("1.0", "1.0"),

          // Basic binary operations
          CreateTestCase("1 + 2", "1 2 +"),
          CreateTestCase("1 - 2", "1 2 -"),
          CreateTestCase("1 * 2", "1 2 *"),
          CreateTestCase("1 / 2", "1 2 /"),
          CreateTestCase("2 % 5", "2 5 %"),

          // String operations
          CreateTestCase("\"Hello\" + \"World\"", "\"Hello\" \"World\" +"),
          CreateTestCase("\"Hello\" + 42", "\"Hello\" 42 +"),
          CreateTestCase("\"Hello\" + 42.0", "\"Hello\" 42.0 +"),
        };
      }
    }

    public static IEnumerable<object[]> PrecedenceData
    {
      get
      {
        return new[]
        {
          // Multiplication has higher precedence than addition
          CreateTestCase("1 + 2 * 3", "1 2 3 * +"),
          CreateTestCase("1 * 2 + 3", "1 2 * 3 +"),

          // Division has higher precedence than subtraction
          CreateTestCase("1 - 2 / 3", "1 2 3 / -"),
          CreateTestCase("2 / 3 - 1", "2 3 / 1 -"),

          // Same precedence operators are left-associative
          CreateTestCase("1 + 2 + 3", "1 2 + 3 +"),
          CreateTestCase("1 - 2 - 3", "1 2 - 3 -"),
          CreateTestCase("1 - 2 + 3", "1 2 - 3 +"),

          // Multiplication and division have same precedence
          CreateTestCase("2 * 3 / 1", "2 3 * 1 /"),
          CreateTestCase("2 / 3 * 1", "2 3 / 1 *"),

          // Complex precedence
          CreateTestCase("1 * 2 + 3 / 4", "1 2 * 3 4 / +"),

          // Parentheses override precedence
          CreateTestCase("(1 + 2) * 3", "1 2 + 3 *"),
          CreateTestCase("1 * (2 + 3)", "1 2 3 + *"),
        };
      }
    }

    public static IEnumerable<object[]> ComplexExpressionData
    {
      get
      {
        return new[]
        {
          // Complex nested expression: ((1 + 2) * 3) / (4 - 5)
          CreateTestCase("((1 + 2) * 3) / (4 - 5)", "1 2 + 3 * 4 5 - /")
        };
      }
    }
  }
}