using Xunit;

namespace EpsilonScript.Tests.Math
{
  [Trait("Category", "Unit")]
  [Trait("Component", "Math")]
  public class Math_IsNearlyEqual
  {
    [Fact]
    public void IsNearlyEqual_DifferentValues_ReturnsFalse()
    {
      Assert.False(EpsilonScript.Math.IsNearlyEqual(0.1f, 0.15f));
    }

    [Fact]
    public void IsNearlyEqual_FloatingPointRoundingError_ReturnsTrue()
    {
      var a = (float)System.Math.PI;
      var b = (float)System.Math.Sqrt(a);
      var c = (float)System.Math.Sqrt(b);
      var d = c * c * c * c;
      Assert.False(a == d); // Direct equality fails due to rounding
      Assert.True(EpsilonScript.Math.IsNearlyEqual(a, d)); // But they are nearly equal
    }

    [Fact]
    public void IsNearlyEqual_VerySmallNumbers_HandlesCorrectly()
    {
      var a = 1e-30f;
      var b = 1e-30f;
      var sum = a + b;
      Assert.True(EpsilonScript.Math.IsNearlyEqual(2e-30f, sum));
    }

    [Fact]
    public void IsNearlyEqual_VeryLargeNumbers_HandlesCorrectly()
    {
      var a = 1e30f;
      var b = 1e30f;
      var sum = a + b;
      Assert.True(EpsilonScript.Math.IsNearlyEqual(2e30f, sum));
    }

    [Fact]
    public void IsNearlyEqual_FloatEpsilonDifference_ReturnsTrue()
    {
      var a = 1.0f;
      var b = 1.0f + float.Epsilon;
      Assert.True(EpsilonScript.Math.IsNearlyEqual(a, b));
    }

    [Theory]
    [InlineData(0.1f, 0.2f, 0.3f)]
    [InlineData(0.1f, 0.1f, 0.2f)]
    [InlineData(1.1f, 2.2f, 3.3f)]
    public void IsNearlyEqual_KnownPrecisionIssues_HandlesCorrectly(float a, float b, float expected)
    {
      var result = a + b;
      // These additions have known precision issues
      Assert.True(EpsilonScript.Math.IsNearlyEqual(expected, result),
        $"Expected {expected} to be nearly equal to {result} (difference: {System.Math.Abs(expected - result)})");
    }

    [Theory]
    [InlineData(float.MaxValue)]
    [InlineData(float.MinValue)]
    [InlineData(float.Epsilon)]
    [InlineData(-float.Epsilon)]
    [InlineData(0.0f)]
    [InlineData(1.0f)]
    [InlineData(-1.0f)]
    public void IsNearlyEqual_IdentityOperations_ReturnsTrue(float value)
    {
      if (float.IsFinite(value))
      {
        Assert.True(EpsilonScript.Math.IsNearlyEqual(value + 0.0f, value));
        Assert.True(EpsilonScript.Math.IsNearlyEqual(value * 1.0f, value));
        if (value != 0.0f)
        {
          Assert.True(EpsilonScript.Math.IsNearlyEqual(value / 1.0f, value));
        }
      }
    }

    [Fact]
    public void IsNearlyEqual_AccumulatedError_ReturnsFalse()
    {
      // This test from Math_FloatPrecision shows accumulated error exceeds epsilon
      var baseValue = 0.1f;
      var sum = 0.0f;
      for (var i = 0; i < 1000000; i++)
      {
        sum += baseValue;
      }

      // After many operations, accumulated error is significant
      Assert.False(EpsilonScript.Math.IsNearlyEqual(baseValue, sum));
    }

    #region Double Precision Tests

    [Fact]
    public void IsNearlyEqual_Double_DifferentValues_ReturnsFalse()
    {
      Assert.False(EpsilonScript.Math.IsNearlyEqual(0.1, 0.15));
    }

    [Fact]
    public void IsNearlyEqual_Double_FloatingPointRoundingError_ReturnsTrue()
    {
      var a = System.Math.PI;
      var b = System.Math.Sqrt(a);
      var c = System.Math.Sqrt(b);
      var d = c * c * c * c;
      Assert.False(a == d); // Direct equality fails due to rounding
      Assert.True(EpsilonScript.Math.IsNearlyEqual(a, d)); // But they are nearly equal
    }

    [Fact]
    public void IsNearlyEqual_Double_VerySmallNumbers_HandlesCorrectly()
    {
      var a = 1e-300;
      var b = 1e-300;
      var sum = a + b;
      Assert.True(EpsilonScript.Math.IsNearlyEqual(2e-300, sum));
    }

    [Fact]
    public void IsNearlyEqual_Double_VeryLargeNumbers_HandlesCorrectly()
    {
      var a = 1e300;
      var b = 1e300;
      var sum = a + b;
      Assert.True(EpsilonScript.Math.IsNearlyEqual(2e300, sum));
    }

    [Fact]
    public void IsNearlyEqual_Double_DoubleEpsilonDifference_ReturnsTrue()
    {
      var a = 1.0;
      var b = 1.0 + double.Epsilon;
      Assert.True(EpsilonScript.Math.IsNearlyEqual(a, b));
    }

    [Theory]
    [InlineData(0.1, 0.2, 0.3)]
    [InlineData(0.1, 0.1, 0.2)]
    [InlineData(1.1, 2.2, 3.3)]
    public void IsNearlyEqual_Double_KnownPrecisionIssues_HandlesCorrectly(double a, double b, double expected)
    {
      var result = a + b;
      // These additions have known precision issues
      Assert.True(EpsilonScript.Math.IsNearlyEqual(expected, result),
        $"Expected {expected} to be nearly equal to {result} (difference: {System.Math.Abs(expected - result)})");
    }

    [Theory]
    [InlineData(double.MaxValue)]
    [InlineData(double.MinValue)]
    [InlineData(double.Epsilon)]
    [InlineData(-double.Epsilon)]
    [InlineData(0.0)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    public void IsNearlyEqual_Double_IdentityOperations_ReturnsTrue(double value)
    {
      if (double.IsFinite(value))
      {
        Assert.True(EpsilonScript.Math.IsNearlyEqual(value + 0.0, value));
        Assert.True(EpsilonScript.Math.IsNearlyEqual(value * 1.0, value));
        if (value != 0.0)
        {
          Assert.True(EpsilonScript.Math.IsNearlyEqual(value / 1.0, value));
        }
      }
    }

    [Fact]
    public void IsNearlyEqual_Double_SignDifference_ReturnsFalse()
    {
      Assert.False(EpsilonScript.Math.IsNearlyEqual(1.0, -1.0));
      Assert.False(EpsilonScript.Math.IsNearlyEqual(0.1, -0.1));
    }

    [Fact]
    public void IsNearlyEqual_Double_PlusMinusZero_ReturnsTrue()
    {
      Assert.True(EpsilonScript.Math.IsNearlyEqual(0.0, -0.0));
      Assert.True(EpsilonScript.Math.IsNearlyEqual(-0.0, 0.0));
    }

    #endregion
  }
}