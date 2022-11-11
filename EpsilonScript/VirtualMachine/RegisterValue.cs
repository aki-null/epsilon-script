using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EpsilonScript.VirtualMachine
{
  [StructLayout(LayoutKind.Explicit)]
  internal struct RegisterValue
  {
    [FieldOffset(0)] public int IntegerValue;
    [FieldOffset(0)] public float FloatValue;
    [FieldOffset(0)] public bool BooleanValue;
    [FieldOffset(sizeof(int))] public RegisterValueType ValueType;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float AsFloat()
    {
      return ValueType switch
      {
        RegisterValueType.Integer => IntegerValue,
        RegisterValueType.Float => FloatValue,
        _ => throw new ArgumentOutOfRangeException(nameof(ValueType))
      };
    }

    public string ResolveString(string[] stringTable, string[] stringRegisters)
    {
      return ValueType switch
      {
        RegisterValueType.Integer => IntegerValue.ToString(),
        RegisterValueType.Float => FloatValue.ToString(CultureInfo.InvariantCulture),
        RegisterValueType.Boolean => BooleanValue ? "true" : "false",
        RegisterValueType.String => stringTable[IntegerValue],
        RegisterValueType.StringStack => stringRegisters[IntegerValue],
        _ => throw new ArgumentOutOfRangeException()
      };
    }

    public ConcreteValue ResolveToConcreteValue(string[] stringTable, string[] stringRegisters)
    {
      return ValueType switch
      {
        RegisterValueType.Integer => new ConcreteValue { Type = Type.Integer, IntegerValue = IntegerValue },
        RegisterValueType.Float => new ConcreteValue { Type = Type.Float, FloatValue = FloatValue },
        RegisterValueType.Boolean => new ConcreteValue { Type = Type.Boolean, BooleanValue = BooleanValue },
        RegisterValueType.String => new ConcreteValue
        {
          Type = Type.String, StringValue = stringTable[IntegerValue]
        },
        RegisterValueType.StringStack => new ConcreteValue
        {
          Type = Type.String, StringValue = stringRegisters[IntegerValue]
        },
        _ => throw new ArgumentOutOfRangeException()
      };
    }
  }
}