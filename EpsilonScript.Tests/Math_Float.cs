using Xunit;

namespace EpsilonScript.Tests
{
  public class Math_Float
  {
    [Fact]
    public void Math_Float_DifferenceDoesNotEqualCorrectly()
    {
      Assert.False(0.1f == 0.15f);
      Assert.False(Math.IsNearlyEqual(0.1f, 0.15f));
    }

    [Fact]
    public void Math_Float_SmallDifferenceEqualsCorrectly()
    {
      var a = (float) System.Math.PI;
      var b = (float) System.Math.Sqrt(a);
      var c = (float) System.Math.Sqrt(b);
      var d = c * c * c * c;
      Assert.False(a == d);
      Assert.True(Math.IsNearlyEqual(a, d));
    }
  }
}