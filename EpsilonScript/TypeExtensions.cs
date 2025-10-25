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
    // Bitmask: bits set for numeric types (1,2,5,6,7)
    // Binary: 11100110 = positions 7,6,5,2,1
    private const uint NumberMask = 0b11100110;

    // Bitmask: bits set for integer types (1,5)
    // Binary: 00100010 = positions 5,1
    private const uint IntegerMask = 0b00100010;

    // Bitmask: bits set for float types (2,6,7)
    // Binary: 11000100 = positions 7,6,2
    private const uint FloatMask = 0b11000100;

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