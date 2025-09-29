using System;
using System.Runtime.InteropServices;

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
        switch (Type)
        {
          case Type.Integer:
            return _integerValue;
          case Type.Float:
            return (int)_floatValue;
          case Type.Boolean:
            return (_booleanValue ? 1 : 0);
          case Type.String:
            if (int.TryParse(_stringValue, out int result))
            {
              return result;
            }

            throw new InvalidCastException($"String value '{_stringValue}' cannot be parsed to an integer");
          default:
            throw new ArgumentOutOfRangeException(nameof(Type), Type, "Unsupported variable type");
        }
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
          case Type.String:
            _stringValue = value.ToString();
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
        switch (Type)
        {
          case Type.Integer:
            return _integerValue;
          case Type.Float:
            return _floatValue;
          case Type.Boolean:
            throw new InvalidCastException("A boolean value cannot be casted to a float value");
          default:
            throw new ArgumentOutOfRangeException(nameof(Type), Type, "Unsupported variable type");
        }
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
        switch (Type)
        {
          case Type.Integer:
            return _integerValue != 0;
          case Type.Float:
            throw new InvalidCastException("A float value cannot be casted to a boolean value");
          case Type.Boolean:
            return _booleanValue;
          default:
            throw new ArgumentOutOfRangeException(nameof(Type), Type, "Unsupported variable type");
        }
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
        switch (Type)
        {
          case Type.String:
            return _stringValue;
          default:
            throw new ArgumentOutOfRangeException(nameof(Type), Type, "Unsupported variable type");
        }
      }
      set
      {
        switch (Type)
        {
          case Type.String:
            _stringValue = value;
            break;
          default:
            throw new ArgumentOutOfRangeException(nameof(Type), Type, "Unsupported variable type");
        }
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
        case Type.Undefined:
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
  }
}