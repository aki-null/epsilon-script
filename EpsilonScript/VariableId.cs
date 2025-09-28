using System;
using EpsilonScript.Helper;

namespace EpsilonScript
{
  /// <summary>
  /// Strongly-typed variable identifier that maintains backwards compatibility with uint.
  /// Provides implicit conversions to/from uint and string.
  /// </summary>
  public readonly struct VariableId : IEquatable<VariableId>, IEquatable<uint>
  {
    private readonly uint _value;

    private VariableId(uint value) => _value = value;

    public static implicit operator VariableId(uint id) => new VariableId(id);
    public static implicit operator uint(VariableId id) => id._value;

    public static implicit operator VariableId(string name) => new VariableId(name.GetUniqueIdentifier());

    public bool Equals(VariableId other) => _value == other._value;
    public bool Equals(uint other) => _value == other;

    public override bool Equals(object obj) => obj switch
    {
      VariableId id => Equals(id),
      uint ui => Equals(ui),
      _ => false
    };

    public override int GetHashCode() => _value.GetHashCode();

    public static bool operator ==(VariableId left, VariableId right) => left.Equals(right);
    public static bool operator !=(VariableId left, VariableId right) => !left.Equals(right);
    public static bool operator ==(VariableId left, uint right) => left.Equals(right);
    public static bool operator !=(VariableId left, uint right) => !left.Equals(right);
    public static bool operator ==(uint left, VariableId right) => right.Equals(left);
    public static bool operator !=(uint left, VariableId right) => !right.Equals(left);

    public override string ToString() => _value.GetStringFromUniqueIdentifier();

    public uint Value => _value;
  }
}