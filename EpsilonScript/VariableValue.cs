using System;

namespace EpsilonScript
{
  public class VariableValue
  {
    public Type Type { get; }

    private int _integerValue;

    public int IntegerValue
    {
      get
      {
        return Type switch
        {
          Type.Integer => _integerValue,
          Type.Float => (int) _floatValue,
          Type.Boolean => (_booleanValue ? 1 : 0),
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

    private float _floatValue;

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
            _integerValue = (int) value;
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

    private bool _booleanValue;

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
  }
}