using System;
using System.Runtime.CompilerServices;

namespace EpsilonScript.Function
{
  /// Efficiently packs parameter types into a single long value for fast comparison.
  ///
  /// Layout (64 bits):
  /// - Bits 0-7:   Parameter count (0-7, supports up to 7 parameters)
  /// - Bits 8-15:  Type of parameter 0
  /// - Bits 16-23: Type of parameter 1
  /// - Bits 24-31: Type of parameter 2
  /// - Bits 32-39: Type of parameter 3
  /// - Bits 40-47: Type of parameter 4
  /// - Bits 48-55: Type of parameter 5
  /// - Bits 56-63: Type of parameter 6
  internal struct PackedParameterTypes : IEquatable<PackedParameterTypes>
  {
    private long _packed;

    private const int CountMask = 0xFF;
    private const int BitsPerType = 8;

    /// Adds a parameter type to the packed value.
    /// Caller must ensure parameter count does not exceed 7.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddType(ExtendedType type)
    {
      var count = (int)(_packed & CountMask);
      _packed += 1; // Increment count in byte 0
      _packed |= (long)(byte)type << ((count + 1) * BitsPerType);
    }

    public int Count
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => (int)(_packed & CountMask);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ExtendedType GetTypeAt(int index)
    {
      if (index < 0 || index >= Count)
      {
        return ExtendedType.Undefined;
      }

      return (ExtendedType)((_packed >> ((index + 1) * BitsPerType)) & 0xFF);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(PackedParameterTypes other)
    {
      return _packed == other._packed;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object obj)
    {
      return obj is PackedParameterTypes other && Equals(other);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
    {
      return _packed.GetHashCode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(PackedParameterTypes left, PackedParameterTypes right)
    {
      return left._packed == right._packed;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(PackedParameterTypes left, PackedParameterTypes right)
    {
      return left._packed != right._packed;
    }

    public override string ToString()
    {
      if (Count == 0) return string.Empty;

      var types = new string[Count];
      for (var i = 0; i < Count; i++)
      {
        types[i] = GetTypeAt(i).ToDebugString();
      }

      return string.Join(", ", types);
    }
  }
}