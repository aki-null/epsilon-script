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