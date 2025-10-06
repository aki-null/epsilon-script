namespace EpsilonScript.AST
{
  public enum ValueType
  {
    Undefined,
    Null,
    Integer,
    Long,
    Float,
    Double,
    Decimal,
    Boolean,
    Tuple,
    String
  }

  public static class ValueTypeExtension
  {
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool IsNumber(this ValueType t)
    {
      return t == ValueType.Integer
             || t == ValueType.Long
             || t == ValueType.Float
             || t == ValueType.Double
             || t == ValueType.Decimal;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool IsInteger(this ValueType t)
    {
      return t == ValueType.Integer || t == ValueType.Long;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool IsFloat(this ValueType t)
    {
      return t == ValueType.Float || t == ValueType.Double || t == ValueType.Decimal;
    }
  }
}