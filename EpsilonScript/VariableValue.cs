using System;
using System.Runtime.InteropServices;
using EpsilonScript.VirtualMachine;

namespace EpsilonScript
{
  [StructLayout(LayoutKind.Explicit)]
  public class VariableValue
  {
    [FieldOffset(0)] private int _integerValue;
    [FieldOffset(0)] private float _floatValue;
    [FieldOffset(0)] private bool _booleanValue;

    [field: FieldOffset(sizeof(int))] public Type Type { get; private set; }

    [FieldOffset(sizeof(int) + sizeof(Type))]
    private string _stringValue;

    public int IntegerValue
    {
      get
      {
        return Type switch
        {
          Type.Integer => _integerValue,
          Type.Float => (int)_floatValue,
          Type.Boolean => _booleanValue ? 1 : 0,
          _ => throw new ArgumentOutOfRangeException(nameof(Type), Type, "Unsupported variable type")
        };
      }
      set
      {
        switch (Type)
        {
          case Type.Integer:
            _integerValue = value;
            break;
          case Type.Float:
            _floatValue = value;
            break;
          case Type.Boolean:
            _booleanValue = value != 0;
            break;
          default:
            throw new ArgumentOutOfRangeException(nameof(Type), Type, "Unsupported variable type");
        }
      }
    }

    public float FloatValue
    {
      get
      {
        return Type switch
        {
          Type.Integer => _integerValue,
          Type.Float => _floatValue,
          Type.Boolean => throw new InvalidCastException("A boolean value cannot be casted to a float value"),
          _ => throw new ArgumentOutOfRangeException(nameof(Type), Type, "Unsupported variable type")
        };
      }
      set
      {
        switch (Type)
        {
          case Type.Integer:
            _integerValue = (int)value;
            break;
          case Type.Float:
            _floatValue = value;
            break;
          case Type.Boolean:
            throw new InvalidCastException("A float value cannot be casted to a boolean value");
          default:
            throw new ArgumentOutOfRangeException(nameof(Type), Type, "Unsupported variable type");
        }
      }
    }

    public bool BooleanValue
    {
      get
      {
        return Type switch
        {
          Type.Integer => _integerValue != 0,
          Type.Float => throw new InvalidCastException("A float value cannot be casted to a boolean value"),
          Type.Boolean => _booleanValue,
          _ => throw new ArgumentOutOfRangeException(nameof(Type), Type, "Unsupported variable type")
        };
      }
      set
      {
        switch (Type)
        {
          case Type.Integer:
            _integerValue = value ? 1 : 0;
            break;
          case Type.Float:
            throw new InvalidCastException("A boolean value cannot be casted to a float value");
          case Type.Boolean:
            _booleanValue = value;
            break;
          default:
            throw new ArgumentOutOfRangeException(nameof(Type), Type, "Unsupported variable type");
        }
      }
    }

    public string StringValue
    {
      get
      {
        return Type switch
        {
          Type.String => _stringValue,
          _ => throw new ArgumentOutOfRangeException(nameof(Type), Type, "Unsupported variable type")
        };
      }
      set
      {
        _stringValue = Type switch
        {
          Type.String => value,
          _ => throw new ArgumentOutOfRangeException(nameof(Type), Type, "Unsupported variable type")
        };
      }
    }

    public VariableValue(Type type)
    {
      Type = type;
    }

    public VariableValue(int value)
    {
      Type = Type.Integer;
      IntegerValue = value;
    }

    public VariableValue(float value)
    {
      Type = Type.Float;
      FloatValue = value;
    }

    public VariableValue(bool value)
    {
      Type = Type.Boolean;
      BooleanValue = value;
    }

    public VariableValue(string value)
    {
      Type = Type.String;
      StringValue = value;
    }

    public void CopyFrom(VariableValue other)
    {
      Type = other.Type;
      switch (Type)
      {
        case Type.Void:
          return;
        case Type.Integer:
          _integerValue = other._integerValue;
          break;
        case Type.Float:
          _floatValue = other._floatValue;
          break;
        case Type.Boolean:
          _integerValue = other._integerValue;
          break;
        case Type.String:
          _stringValue = other._stringValue;
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    internal unsafe void LoadToRegister(RegisterValue* regPtr, string[] stringRegisters, int index)
    {
      var targetPtr = regPtr + index;
      switch (Type)
      {
        case Type.Integer:
          targetPtr->ValueType = RegisterValueType.Integer;
          targetPtr->IntegerValue = _integerValue;
          break;
        case Type.Float:
          targetPtr->ValueType = RegisterValueType.Float;
          targetPtr->FloatValue = _floatValue;
          break;
        case Type.Boolean:
          targetPtr->ValueType = RegisterValueType.Boolean;
          targetPtr->BooleanValue = _booleanValue;
          break;
        case Type.String:
          stringRegisters[index] = _stringValue;
          targetPtr->ValueType = RegisterValueType.StringStack;
          break;
        case Type.Void:
        default:
          throw new ArgumentOutOfRangeException(nameof(Type), "Unsupported variable type");
      }
    }
  }
}