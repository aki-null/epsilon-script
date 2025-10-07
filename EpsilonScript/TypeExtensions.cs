namespace EpsilonScript
{
  public static class TypeExtensions
  {
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool IsNumber(this Type t)
    {
      return t == Type.Integer
             || t == Type.Long
             || t == Type.Float
             || t == Type.Double
             || t == Type.Decimal;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool IsInteger(this Type t)
    {
      return t == Type.Integer || t == Type.Long;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool IsFloat(this Type t)
    {
      return t == Type.Float || t == Type.Double || t == Type.Decimal;
    }
  }
}