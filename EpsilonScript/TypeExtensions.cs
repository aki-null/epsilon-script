namespace EpsilonScript
{
  internal static class ExtendedTypeExtensions
  {
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool IsNumber(this ExtendedType t)
    {
      return t >= ExtendedType.Integer && t <= ExtendedType.Decimal;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool IsInteger(this ExtendedType t)
    {
      return t == ExtendedType.Integer || t == ExtendedType.Long;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool IsFloat(this ExtendedType t)
    {
      return t >= ExtendedType.Float && t <= ExtendedType.Decimal;
    }
  }
}