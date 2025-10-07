using System.Runtime.CompilerServices;
#if UNITY_2018_1_OR_NEWER
using Unity.Collections.LowLevel.Unsafe;
#endif

namespace EpsilonScript
{
  public static class Math
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int UnsafeFloatAsInt32(float f)
    {
#if UNITY_2018_1_OR_NEWER
      return UnsafeUtility.As<float, int>(ref f);
#else
      return Unsafe.As<float, int>(ref f);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long UnsafeDoubleAsInt64(double d)
    {
#if UNITY_2018_1_OR_NEWER
      return UnsafeUtility.As<double, long>(ref d);
#else
      return Unsafe.As<double, long>(ref d);
#endif
    }

    // https://randomascii.wordpress.com/2012/02/25/comparing-floating-point-numbers-2012-edition/
    public static bool IsNearlyEqual(float a, float b, int maxUlpsDiff = 2)
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

    // ULPs-based comparison for double precision
    public static bool IsNearlyEqual(double a, double b, long maxUlpsDiff = 2)
    {
      var aLong = UnsafeDoubleAsInt64(a);
      var bLong = UnsafeDoubleAsInt64(b);

      // Different signs means they do not match
      if (aLong < 0 != bLong < 0)
      {
        // Check for equality to make sure +0==-0
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        return a == b;
      }

      // Find the difference in ULPs
      var ulpsDiff = System.Math.Abs(aLong - bLong);
      return ulpsDiff <= maxUlpsDiff;
    }
  }
}