using System;
using System.Runtime.CompilerServices;
using EpsilonScript.AST;
#if UNITY_2018_1_OR_NEWER
using Unity.Collections.LowLevel.Unsafe;
#endif

namespace EpsilonScript.Function
{
  internal static class TypeTraits<T>
  {
    private static readonly Func<Node, T> ReadFunc;
    private static readonly Func<T, int> ToIntFunc;
    private static readonly Func<T, long> ToLongFunc;
    private static readonly Func<T, float> ToFloatFunc;
    private static readonly Func<T, double> ToDoubleFunc;
    private static readonly Func<T, decimal> ToDecimalFunc;
    private static readonly Func<T, bool> ToBoolFunc;
    private static readonly Func<T, string> ToStringFunc;

    // ReSharper disable once StaticMemberInGenericType
    public static Type ValueType { get; }

    static TypeTraits()
    {
      ReadFunc = ThrowRead;
      ToIntFunc = ThrowToInt;
      ToLongFunc = ThrowToLong;
      ToFloatFunc = ThrowToFloat;
      ToDoubleFunc = ThrowToDouble;
      ToDecimalFunc = ThrowToDecimal;
      ToBoolFunc = ThrowToBool;
      ToStringFunc = ThrowToString;

      if (typeof(T) == typeof(int))
      {
        ValueType = Type.Integer;
        ReadFunc = ReadInt;
        ToIntFunc = ToIntIdentity;
        ToLongFunc = ToIntAsLong;
        return;
      }

      if (typeof(T) == typeof(long))
      {
        ValueType = Type.Long;
        ReadFunc = ReadLong;
        ToIntFunc = ToLongAsInt;
        ToLongFunc = ToLongIdentity;
        return;
      }

      if (typeof(T) == typeof(float))
      {
        ValueType = Type.Float;
        ReadFunc = ReadFloat;
        ToFloatFunc = ToFloatIdentity;
        ToDoubleFunc = ToFloatAsDouble;
        return;
      }

      if (typeof(T) == typeof(double))
      {
        ValueType = Type.Double;
        ReadFunc = ReadDouble;
        ToFloatFunc = ToDoubleAsFloat;
        ToDoubleFunc = ToDoubleIdentity;
        return;
      }

      if (typeof(T) == typeof(decimal))
      {
        ValueType = Type.Decimal;
        ReadFunc = ReadDecimal;
        ToDecimalFunc = ToDecimalIdentity;
        return;
      }

      if (typeof(T) == typeof(bool))
      {
        ValueType = Type.Boolean;
        ReadFunc = ReadBool;
        ToBoolFunc = ToBoolIdentity;
        return;
      }

      if (typeof(T) == typeof(string))
      {
        ValueType = Type.String;
        ReadFunc = ReadString;
        ToStringFunc = ToStringIdentity;
        return;
      }

      throw new NotSupportedException($"Type {typeof(T)} is not supported by EpsilonScript custom functions");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Read(Node node)
    {
      return ReadFunc(node);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToInt(T value)
    {
      return ToIntFunc(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToLong(T value)
    {
      return ToLongFunc(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ToFloat(T value)
    {
      return ToFloatFunc(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ToDouble(T value)
    {
      return ToDoubleFunc(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal ToDecimal(T value)
    {
      return ToDecimalFunc(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ToBool(T value)
    {
      return ToBoolFunc(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToStringValue(T value)
    {
      return ToStringFunc(value);
    }

    private static T ThrowRead(Node _)
    {
      throw new InvalidOperationException($"Unsupported script type: {ValueType}");
    }

    private static int ThrowToInt(T _)
    {
      throw new InvalidOperationException("Attempted to extract integer value from non-integer type");
    }

    private static long ThrowToLong(T _)
    {
      throw new InvalidOperationException("Attempted to extract long value from non-long type");
    }

    private static float ThrowToFloat(T _)
    {
      throw new InvalidOperationException("Attempted to extract float value from non-float type");
    }

    private static double ThrowToDouble(T _)
    {
      throw new InvalidOperationException("Attempted to extract double value from non-double type");
    }

    private static decimal ThrowToDecimal(T _)
    {
      throw new InvalidOperationException("Attempted to extract decimal value from non-decimal type");
    }

    private static bool ThrowToBool(T _)
    {
      throw new InvalidOperationException("Attempted to extract boolean value from non-boolean type");
    }

    private static string ThrowToString(T _)
    {
      throw new InvalidOperationException("Attempted to extract string value from non-string type");
    }

    private static T ReadInt(Node node)
    {
      var value = node.IntegerValue;
#if UNITY_2018_1_OR_NEWER
      return UnsafeUtility.As<int, T>(ref value);
#else
      return Unsafe.As<int, T>(ref value);
#endif
    }

    private static int ToIntIdentity(T value)
    {
      var temp = value;
#if UNITY_2018_1_OR_NEWER
      return UnsafeUtility.As<T, int>(ref temp);
#else
      return Unsafe.As<T, int>(ref temp);
#endif
    }

    private static T ReadFloat(Node node)
    {
      var value = node.FloatValue;
#if UNITY_2018_1_OR_NEWER
      return UnsafeUtility.As<float, T>(ref value);
#else
      return Unsafe.As<float, T>(ref value);
#endif
    }

    private static float ToFloatIdentity(T value)
    {
      var temp = value;
#if UNITY_2018_1_OR_NEWER
      return UnsafeUtility.As<T, float>(ref temp);
#else
      return Unsafe.As<T, float>(ref temp);
#endif
    }

    private static T ReadBool(Node node)
    {
      var value = node.BooleanValue;
#if UNITY_2018_1_OR_NEWER
      return UnsafeUtility.As<bool, T>(ref value);
#else
      return Unsafe.As<bool, T>(ref value);
#endif
    }

    private static bool ToBoolIdentity(T value)
    {
      var temp = value;
#if UNITY_2018_1_OR_NEWER
      return UnsafeUtility.As<T, bool>(ref temp);
#else
      return Unsafe.As<T, bool>(ref temp);
#endif
    }

    private static T ReadLong(Node node)
    {
      var value = node.LongValue;
#if UNITY_2018_1_OR_NEWER
      return UnsafeUtility.As<long, T>(ref value);
#else
      return Unsafe.As<long, T>(ref value);
#endif
    }

    private static int ToLongAsInt(T value)
    {
      var temp = value;
#if UNITY_2018_1_OR_NEWER
      return (int)UnsafeUtility.As<T, long>(ref temp);
#else
      return (int)Unsafe.As<T, long>(ref temp);
#endif
    }

    private static T ReadDouble(Node node)
    {
      var value = node.DoubleValue;
#if UNITY_2018_1_OR_NEWER
      return UnsafeUtility.As<double, T>(ref value);
#else
      return Unsafe.As<double, T>(ref value);
#endif
    }

    private static float ToDoubleAsFloat(T value)
    {
      var temp = value;
#if UNITY_2018_1_OR_NEWER
      return (float)UnsafeUtility.As<T, double>(ref temp);
#else
      return (float)Unsafe.As<T, double>(ref temp);
#endif
    }

    private static T ReadDecimal(Node node)
    {
      var value = node.DecimalValue;
#if UNITY_2018_1_OR_NEWER
      return UnsafeUtility.As<decimal, T>(ref value);
#else
      return Unsafe.As<decimal, T>(ref value);
#endif
    }

    private static T ReadString(Node node)
    {
      return (T)(object)node.StringValue;
    }

    private static long ToIntAsLong(T value)
    {
      var temp = value;
#if UNITY_2018_1_OR_NEWER
      return UnsafeUtility.As<T, int>(ref temp);
#else
      return Unsafe.As<T, int>(ref temp);
#endif
    }

    private static long ToLongIdentity(T value)
    {
      var temp = value;
#if UNITY_2018_1_OR_NEWER
      return UnsafeUtility.As<T, long>(ref temp);
#else
      return Unsafe.As<T, long>(ref temp);
#endif
    }

    private static double ToFloatAsDouble(T value)
    {
      var temp = value;
#if UNITY_2018_1_OR_NEWER
      return UnsafeUtility.As<T, float>(ref temp);
#else
      return Unsafe.As<T, float>(ref temp);
#endif
    }

    private static double ToDoubleIdentity(T value)
    {
      var temp = value;
#if UNITY_2018_1_OR_NEWER
      return UnsafeUtility.As<T, double>(ref temp);
#else
      return Unsafe.As<T, double>(ref temp);
#endif
    }

    private static decimal ToDecimalIdentity(T value)
    {
      var temp = value;
#if UNITY_2018_1_OR_NEWER
      return UnsafeUtility.As<T, decimal>(ref temp);
#else
      return Unsafe.As<T, decimal>(ref temp);
#endif
    }

    private static string ToStringIdentity(T value)
    {
      return (string)(object)value;
    }
  }
}