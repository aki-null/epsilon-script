using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.RpnConverter
{
  public class RPNConverter_Semicolon : RpnConverterTestBase
  {
    [Theory]
    [MemberData(nameof(CorrectData))]
    internal void RPNConverter_ParsesSemicolon_Correctly(Element[] input, Element[] expected)
    {
      AssertRpnSucceeds(input, expected);
    }

    public static IEnumerable<object[]> CorrectData
    {
      get
      {
        return new[]
        {
          // Semicolon acts as operator - moves to end in RPN (lowest precedence)
          CreateTestCase("1 ; 2", "1 2 ;"),
          CreateTestCase("1 ; 2 ; 3", "1 2 ; 3 ;"),

          // Semicolon with expressions (has lowest precedence)
          CreateTestCase("x = 1 ; y = 2", "x 1 = y 2 = ;"),
          CreateTestCase("1 + 2 ; 3 * 4", "1 2 + 3 4 * ;"),
        };
      }
    }
  }
}