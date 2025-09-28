using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;
using EpsilonScript.Tests.TestInfrastructure.Fakes;

namespace EpsilonScript.Tests.RpnConverter
{
  [Trait("Category", "Unit")]
  [Trait("Component", "RpnConverter")]
  public class RPNConverter_Assignment : RpnConverterTestBase
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    public void RPNConverter_ParsesAssignment_Correctly(Element[] input, Element[] expected)
    {
      AssertRpnSucceeds(input, expected);
    }

    public static IEnumerable<object[]> CorrectData
    {
      get
      {
        return new[]
        {
          // Single assignment operators pass through unchanged
          CreateTestCase("=", "="),
          CreateTestCase("+=", "+="),
          CreateTestCase("-=", "-="),
          CreateTestCase("*=", "*="),
          CreateTestCase("/=", "/="),

          // Simple assignment: left = right
          CreateTestCase("left = right", "left right ="),
          CreateTestCase("left += right", "left right +="),
          CreateTestCase("left -= right", "left right -="),
          CreateTestCase("left *= right", "left right *="),
          CreateTestCase("left /= right", "left right /="),

          // Assignment with expression on right side
          CreateTestCase("left = right1 + right2", "left right1 right2 + ="),
          CreateTestCase("left += right1 + right2", "left right1 right2 + +="),
          CreateTestCase("left -= right1 + right2", "left right1 right2 + -="),
          CreateTestCase("left *= right1 + right2", "left right1 right2 + *="),
          CreateTestCase("left /= right1 + right2", "left right1 right2 + /="),

          // Assignment with more complex expressions
          CreateTestCase("left = right1 * right2 + right3", "left right1 right2 * right3 + ="),
          CreateTestCase("left += right1 * right2 + right3", "left right1 right2 * right3 + +="),

          // Multiple assignment operators (though probably not valid syntax, test RPN conversion)
          CreateTestCase("left = middle += right", "left middle right += ="),
        };
      }
    }
  }
}