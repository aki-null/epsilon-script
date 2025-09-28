namespace EpsilonScript
{
  public static class Math
  {
    private static unsafe int UnsafeFloatAsInt32(float f)
    {
      return *((int*)&f);
    }

    // https://randomascii.wordpress.com/2012/02/25/comparing-floating-point-numbers-2012-edition/
    public static bool IsNearlyEqual(float a, float b, int maxUlpsDiff = 3)
    {
      var aInt = UnsafeFloatAsInt32(a);
      var bInt = UnsafeFloatAsInt32(b);

      // Different signs means they do not match
      if (aInt < 0 != bInt < 0)
      {
        // Check for equality to make sure +0==-0
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        return a == b;
      }

      // Find the difference in ULPs
      var ulpsDiff = System.Math.Abs(aInt - bInt);
      return ulpsDiff <= maxUlpsDiff;
    }
  }
}