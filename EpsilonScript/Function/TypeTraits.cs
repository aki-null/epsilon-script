using System;
using EpsilonScript.VirtualMachine;

namespace EpsilonScript.Function
{
  internal static class TypeTraits<T>
  {
    // ReSharper disable once StaticMemberInGenericType
    public static Type ValueType { get; }

    /// <summary>
    /// Conversion strategy for <typeparamref name="T"/>. Hot paths should cache this in an instance field:
    /// static fields of a generic class cost a runtime generic context lookup and class initialization check per
    /// access under IL2CPP shared generics.
    /// </summary>
    public static readonly ValueConverter<T> Converter;

    static TypeTraits()
    {
      if (typeof(T) == typeof(int))
      {
        ValueType = Type.Integer;
        Converter = (ValueConverter<T>)(object)IntConverter.Instance;
        return;
      }

      if (typeof(T) == typeof(long))
      {
        ValueType = Type.Long;
        Converter = (ValueConverter<T>)(object)LongConverter.Instance;
        return;
      }

      if (typeof(T) == typeof(float))
      {
        ValueType = Type.Float;
        Converter = (ValueConverter<T>)(object)FloatConverter.Instance;
        return;
      }

      if (typeof(T) == typeof(double))
      {
        ValueType = Type.Double;
        Converter = (ValueConverter<T>)(object)DoubleConverter.Instance;
        return;
      }

      if (typeof(T) == typeof(decimal))
      {
        ValueType = Type.Decimal;
        Converter = (ValueConverter<T>)(object)DecimalConverter.Instance;
        return;
      }

      if (typeof(T) == typeof(bool))
      {
        ValueType = Type.Boolean;
        Converter = (ValueConverter<T>)(object)BoolConverter.Instance;
        return;
      }

      if (typeof(T) == typeof(string))
      {
        ValueType = Type.String;
        Converter = (ValueConverter<T>)(object)StringConverter.Instance;
        return;
      }

      throw new NotSupportedException($"Type {typeof(T)} is not supported by EpsilonScript custom functions");
    }
  }

  /// <summary>
  /// Converts between script values and <typeparamref name="T"/>. Each supported type has a sealed, non-generic
  /// subclass, so a call is one virtual dispatch into a direct field read with no generic context lookups.
  /// </summary>
  internal abstract class ValueConverter<T>
  {
    public abstract T Read(RegisterProgram program, int register);
    public abstract void Write(RegisterProgram program, int register, T value);
    public abstract T FromVariableValue(VariableValue variableValue);
  }

  internal sealed class IntConverter : ValueConverter<int>
  {
    public static readonly IntConverter Instance = new IntConverter();

    public override int Read(RegisterProgram program, int register) => program.ReadInteger(register);

    public override void Write(RegisterProgram program, int register, int value) =>
      program.WriteInteger(register, value);

    public override int FromVariableValue(VariableValue variableValue) => variableValue.IntegerValue;
  }

  internal sealed class LongConverter : ValueConverter<long>
  {
    public static readonly LongConverter Instance = new LongConverter();

    public override long Read(RegisterProgram program, int register) => program.ReadLong(register);

    public override void Write(RegisterProgram program, int register, long value) =>
      program.WriteLong(register, value);

    public override long FromVariableValue(VariableValue variableValue) => variableValue.LongValue;
  }

  internal sealed class FloatConverter : ValueConverter<float>
  {
    public static readonly FloatConverter Instance = new FloatConverter();

    public override float Read(RegisterProgram program, int register) => program.ReadFloat(register);

    public override void Write(RegisterProgram program, int register, float value) =>
      program.WriteFloat(register, value);

    public override float FromVariableValue(VariableValue variableValue) => variableValue.FloatValue;
  }

  internal sealed class DoubleConverter : ValueConverter<double>
  {
    public static readonly DoubleConverter Instance = new DoubleConverter();

    public override double Read(RegisterProgram program, int register) => program.ReadDouble(register);

    public override void Write(RegisterProgram program, int register, double value) =>
      program.WriteDouble(register, value);

    public override double FromVariableValue(VariableValue variableValue) => variableValue.DoubleValue;
  }

  internal sealed class DecimalConverter : ValueConverter<decimal>
  {
    public static readonly DecimalConverter Instance = new DecimalConverter();

    public override decimal Read(RegisterProgram program, int register) => program.ReadDecimal(register);

    public override void Write(RegisterProgram program, int register, decimal value) =>
      program.WriteDecimal(register, value);

    public override decimal FromVariableValue(VariableValue variableValue) => variableValue.DecimalValue;
  }

  internal sealed class BoolConverter : ValueConverter<bool>
  {
    public static readonly BoolConverter Instance = new BoolConverter();

    public override bool Read(RegisterProgram program, int register) => program.ReadBoolean(register);

    public override void Write(RegisterProgram program, int register, bool value) =>
      program.WriteBoolean(register, value);

    public override bool FromVariableValue(VariableValue variableValue) => variableValue.BooleanValue;
  }

  internal sealed class StringConverter : ValueConverter<string>
  {
    public static readonly StringConverter Instance = new StringConverter();

    public override string Read(RegisterProgram program, int register) => program.ReadString(register);

    public override void Write(RegisterProgram program, int register, string value) =>
      program.WriteString(register, value);

    public override string FromVariableValue(VariableValue variableValue) => variableValue.StringValue;
  }
}
