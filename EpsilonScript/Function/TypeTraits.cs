using System;
using System.Runtime.CompilerServices;
using EpsilonScript.AST;
using ScriptType = EpsilonScript.Type;
#if UNITY_2018_1_OR_NEWER
using Unity.Collections.LowLevel.Unsafe;
#endif

namespace EpsilonScript.Function
{
  internal static class TypeTraits<T>
  {
    private static readonly Func<Node, T> ReadFunc;
    private static readonly Func<T, int> ToIntFunc;
    private static readonly Func<T, float> ToFloatFunc;
    private static readonly Func<T, bool> ToBoolFunc;
    private static readonly Func<T, string> ToStringFunc;

    public static ScriptType ScriptType { get; }

    static TypeTraits()
    {
      ReadFunc = ThrowRead;
      ToIntFunc = ThrowToInt;
      ToFloatFunc = ThrowToFloat;
      ToBoolFunc = ThrowToBool;
      ToStringFunc = ThrowToString;

      if (typeof(T) == typeof(int))
      {
        ScriptType = ScriptType.Integer;
        ReadFunc = ReadInt;
        ToIntFunc = ToIntIdentity;
        return;
      }

      if (typeof(T) == typeof(float))
      {
        ScriptType = ScriptType.Float;
        ReadFunc = ReadFloat;
        ToFloatFunc = ToFloatIdentity;
        return;
      }

      if (typeof(T) == typeof(bool))
      {
        ScriptType = ScriptType.Boolean;
        ReadFunc = ReadBool;
        ToBoolFunc = ToBoolIdentity;
        return;
      }

      if (typeof(T) == typeof(string))
      {
        ScriptType = ScriptType.String;
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
    public static float ToFloat(T value)
    {
      return ToFloatFunc(value);
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
      throw new InvalidOperationException($"Unsupported script type: {ScriptType}");
    }

    private static int ThrowToInt(T _)
    {
      throw new InvalidOperationException("Attempted to extract integer value from non-integer type");
    }

    private static float ThrowToFloat(T _)
    {
      throw new InvalidOperationException("Attempted to extract float value from non-float type");
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

    private static T ReadString(Node node)
    {
      return (T)(object)node.StringValue;
    }

    private static string ToStringIdentity(T value)
    {
      return (string)(object)value;
    }
  }
}