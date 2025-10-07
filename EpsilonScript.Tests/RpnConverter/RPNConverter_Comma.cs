using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.RpnConverter
{
  public class RPNConverter_Comma : RpnConverterTestBase
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    internal void RPNConverter_ParsesComma_Correctly(Element[] input, Element[] expected)
    {
      AssertRpnSucceeds(input, expected);
    }

    public static IEnumerable<object[]> CorrectData
    {
      get
      {
        return new[]
        {
          // Comma acts as operator - moves to end in RPN
          CreateTestCase("1 , 2", "1 2 ,"),
          CreateTestCase("1 , 2 , 3", "1 2 , 3 ,"),
          CreateTestCase("var1 , var2", "var1 var2 ,"),

          // Comma with expressions (function arguments, etc) - comma has lowest precedence
          CreateTestCase("1 + 2 , 3 * 4", "1 2 + 3 4 * ,"),
          CreateTestCase("func , arg1 , arg2", "func arg1 , arg2 ,"),
        };
      }
    }
  }
}