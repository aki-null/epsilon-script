namespace EpsilonScript
{
  internal static class TypeExtensions
  {
    public static string ToDebugString(this Type t)
    {
      return t switch
      {
        Type.Undefined => "void",
        Type.Integer => "int",
        Type.Long => "long",
        Type.Float => "float",
        Type.Double => "double",
        Type.Decimal => "decimal",
        Type.Boolean => "bool",
        Type.String => "string",
        _ => throw new System.ArgumentOutOfRangeException(nameof(t), $"Unknown type: {t}"),
      };
    }
  }

  internal static class ExtendedTypeExtensions
  {
    private const uint NumberMask =
      (1U << (int)ExtendedType.Integer) |
      (1U << (int)ExtendedType.Long) |
      (1U << (int)ExtendedType.Float) |
      (1U << (int)ExtendedType.Double) |
      (1U << (int)ExtendedType.Decimal);

    private const uint IntegerMask =
      (1U << (int)ExtendedType.Integer) |
      (1U << (int)ExtendedType.Long);

    private const uint FloatMask =
      (1U << (int)ExtendedType.Float) |
      (1U << (int)ExtendedType.Double) |
      (1U << (int)ExtendedType.Decimal);

    public static string ToDebugString(this ExtendedType t)
    {
      return t switch
      {
        ExtendedType.Undefined => "void",
        ExtendedType.Integer => "int",
        ExtendedType.Long => "long",
        ExtendedType.Float => "float",
        ExtendedType.Double => "double",
        ExtendedType.Decimal => "decimal",
        ExtendedType.Boolean => "bool",
        ExtendedType.String => "string",
        ExtendedType.Null => "null",
        ExtendedType.Tuple => "tuple",
        _ => throw new System.ArgumentOutOfRangeException(nameof(t), $"Unknown type: {t}"),
      };
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool IsNumber(this ExtendedType t)
    {
      return ((NumberMask >> (int)t) & 1) != 0;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool IsInteger(this ExtendedType t)
    {
      return ((IntegerMask >> (int)t) & 1) != 0;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool IsFloat(this ExtendedType t)
    {
      return ((FloatMask >> (int)t) & 1) != 0;
    }
  }
}