using System;
using System.Runtime.InteropServices;
using System.Globalization;

namespace EpsilonScript
{
  public class VariableValue
  {
    [StructLayout(LayoutKind.Explicit)]
    private struct ValueUnion
    {
      [FieldOffset(0)] public long IntValue; // For Integer and Long (8 bytes)
      [FieldOffset(0)] public double FloatValue; // For Float and Double (8 bytes)
      [FieldOffset(0)] public bool BoolValue; // For Boolean (1 byte, 7 bytes unused)
    }

    private Type _type;
    private ValueUnion _value; // 8-byte union
    private decimal _decimalValue; // Separate field for Decimal (16 bytes)
    private string _stringValue; // Reference type

    public Type Type => _type;

    public int IntegerValue
    {
      get
      {
        switch (_type)
        {
          case Type.Integer:
          case Type.Long:
            return (int)_value.IntValue;
          case Type.Float:
          case Type.Double:
            if (double.IsNaN(_value.FloatValue))
              throw new InvalidCastException("Cannot convert NaN to integer");
            if (double.IsInfinity(_value.FloatValue))
              throw new InvalidCastException("Cannot convert Infinity to integer");
            return (int)_value.FloatValue;
          case Type.Decimal:
            return (int)_decimalValue;
          case Type.Boolean:
            return _value.BoolValue ? 1 : 0;
          case Type.String:
            if (int.TryParse(_stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
              return result;
            throw new InvalidCastException($"Cannot convert string '{_stringValue}' to integer");
          default:
            throw new InvalidCastException($"Cannot convert {_type} to integer");
        }
      }
      set
      {
        switch (_type)
        {
          case Type.Undefined:
            _type = Type.Integer;
            _value.IntValue = value;
            break;
          case Type.Integer:
          case Type.Long:
            _value.IntValue = value;
            break;
          case Type.Float:
          case Type.Double:
            _value.FloatValue = value;
            break;
          case Type.Decimal:
            _decimalValue = value;
            break;
          case Type.Boolean:
            _value.BoolValue = value != 0;
            break;
          case Type.String:
            _stringValue = value.ToString(CultureInfo.InvariantCulture);
            break;
          default:
            throw new InvalidCastException($"Cannot assign integer to {_type}");
        }
      }
    }

    public long LongValue
    {
      get
      {
        switch (_type)
        {
          case Type.Integer:
          case Type.Long:
            return _value.IntValue;
          case Type.Float:
          case Type.Double:
            if (double.IsNaN(_value.FloatValue))
              throw new InvalidCastException("Cannot convert NaN to long");
            if (double.IsInfinity(_value.FloatValue))
              throw new InvalidCastException("Cannot convert Infinity to long");
            return (long)_value.FloatValue;
          case Type.Decimal:
            return (long)_decimalValue;
          case Type.Boolean:
            return _value.BoolValue ? 1L : 0L;
          case Type.String:
            if (long.TryParse(_stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
              return result;
            throw new InvalidCastException($"Cannot convert string '{_stringValue}' to long");
          default:
            throw new InvalidCastException($"Cannot convert {_type} to long");
        }
      }
      set
      {
        switch (_type)
        {
          case Type.Undefined:
            _type = Type.Long;
            _value.IntValue = value;
            break;
          case Type.Integer:
          case Type.Long:
            _value.IntValue = value;
            break;
          case Type.Float:
          case Type.Double:
            _value.FloatValue = value;
            break;
          case Type.Decimal:
            _decimalValue = value;
            break;
          case Type.Boolean:
            _value.BoolValue = value != 0;
            break;
          case Type.String:
            _stringValue = value.ToString(CultureInfo.InvariantCulture);
            break;
          default:
            throw new InvalidCastException($"Cannot assign long to {_type}");
        }
      }
    }

    public float FloatValue
    {
      get
      {
        switch (_type)
        {
          case Type.Integer:
          case Type.Long:
            return _value.IntValue;
          case Type.Float:
          case Type.Double:
            return (float)_value.FloatValue;
          case Type.Decimal:
            return (float)_decimalValue;
          case Type.Boolean:
            return _value.BoolValue ? 1.0f : 0.0f;
          case Type.String:
            if (float.TryParse(_stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
              return result;
            throw new InvalidCastException($"Cannot convert string '{_stringValue}' to float");
          default:
            throw new InvalidCastException($"Cannot convert {_type} to float");
        }
      }
      set
      {
        switch (_type)
        {
          case Type.Undefined:
            _type = Type.Float;
            _value.FloatValue = value;
            break;
          case Type.Integer:
          case Type.Long:
            _value.IntValue = (long)value;
            break;
          case Type.Float:
          case Type.Double:
            _value.FloatValue = value;
            break;
          case Type.Decimal:
            _decimalValue = (decimal)value;
            break;
          case Type.Boolean:
            _value.BoolValue = value != 0.0f;
            break;
          case Type.String:
            _stringValue = value.ToString(CultureInfo.InvariantCulture);
            break;
          default:
            throw new InvalidCastException($"Cannot assign float to {_type}");
        }
      }
    }

    public double DoubleValue
    {
      get
      {
        switch (_type)
        {
          case Type.Integer:
          case Type.Long:
            return _value.IntValue;
          case Type.Float:
          case Type.Double:
            return _value.FloatValue;
          case Type.Decimal:
            return (double)_decimalValue;
          case Type.Boolean:
            return _value.BoolValue ? 1.0 : 0.0;
          case Type.String:
            if (double.TryParse(_stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
              return result;
            throw new InvalidCastException($"Cannot convert string '{_stringValue}' to double");
          default:
            throw new InvalidCastException($"Cannot convert {_type} to double");
        }
      }
      set
      {
        switch (_type)
        {
          case Type.Undefined:
            _type = Type.Double;
            _value.FloatValue = value;
            break;
          case Type.Integer:
          case Type.Long:
            _value.IntValue = (long)value;
            break;
          case Type.Float:
          case Type.Double:
            _value.FloatValue = value;
            break;
          case Type.Decimal:
            _decimalValue = (decimal)value;
            break;
          case Type.Boolean:
            _value.BoolValue = value != 0.0;
            break;
          case Type.String:
            _stringValue = value.ToString(CultureInfo.InvariantCulture);
            break;
          default:
            throw new InvalidCastException($"Cannot assign double to {_type}");
        }
      }
    }

    public decimal DecimalValue
    {
      get
      {
        switch (_type)
        {
          case Type.Integer:
          case Type.Long:
            return _value.IntValue;
          case Type.Float:
          case Type.Double:
            return (decimal)_value.FloatValue;
          case Type.Decimal:
            return _decimalValue;
          case Type.Boolean:
            return _value.BoolValue ? 1m : 0m;
          case Type.String:
            if (decimal.TryParse(_stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
              return result;
            throw new InvalidCastException($"Cannot convert string '{_stringValue}' to decimal");
          default:
            throw new InvalidCastException($"Cannot convert {_type} to decimal");
        }
      }
      set
      {
        switch (_type)
        {
          case Type.Undefined:
            _type = Type.Decimal;
            _decimalValue = value;
            break;
          case Type.Integer:
          case Type.Long:
            _value.IntValue = (long)value;
            break;
          case Type.Float:
          case Type.Double:
            _value.FloatValue = (double)value;
            break;
          case Type.Decimal:
            _decimalValue = value;
            break;
          case Type.Boolean:
            _value.BoolValue = value != 0m;
            break;
          case Type.String:
            _stringValue = value.ToString(CultureInfo.InvariantCulture);
            break;
          default:
            throw new InvalidCastException($"Cannot assign decimal to {_type}");
        }
      }
    }

    public bool BooleanValue
    {
      get
      {
        switch (_type)
        {
          case Type.Boolean:
            return _value.BoolValue;
          case Type.Integer:
          case Type.Long:
            return _value.IntValue != 0;
          case Type.Float:
          case Type.Double:
            return _value.FloatValue != 0.0;
          case Type.Decimal:
            return _decimalValue != 0m;
          default:
            throw new InvalidCastException($"Cannot convert {_type} to boolean");
        }
      }
      set
      {
        switch (_type)
        {
          case Type.Undefined:
            _type = Type.Boolean;
            _value.BoolValue = value;
            break;
          case Type.Integer:
          case Type.Long:
            _value.IntValue = value ? 1 : 0;
            break;
          case Type.Boolean:
            _value.BoolValue = value;
            break;
          case Type.Float:
          case Type.Double:
            _value.FloatValue = value ? 1.0 : 0.0;
            break;
          case Type.Decimal:
            _decimalValue = value ? 1m : 0m;
            break;
          default:
            throw new InvalidCastException($"Cannot assign boolean to {_type}");
        }
      }
    }

    public string StringValue
    {
      get
      {
        switch (_type)
        {
          case Type.String:
            return _stringValue;
          case Type.Integer:
          case Type.Long:
            return _value.IntValue.ToString(CultureInfo.InvariantCulture);
          case Type.Float:
            return ((float)_value.FloatValue).ToString(CultureInfo.InvariantCulture);
          case Type.Double:
            return _value.FloatValue.ToString(CultureInfo.InvariantCulture);
          case Type.Decimal:
            return _decimalValue.ToString(CultureInfo.InvariantCulture);
          case Type.Boolean:
            return _value.BoolValue.ToString();
          default:
            throw new InvalidCastException($"Cannot convert {_type} to string");
        }
      }
      set
      {
        switch (_type)
        {
          case Type.Undefined:
            _type = Type.String;
            _stringValue = value;
            break;
          case Type.String:
            _stringValue = value;
            break;
          default:
            throw new InvalidCastException($"Cannot assign string to {_type}");
        }
      }
    }

    public VariableValue(Type type)
    {
      _type = type;
    }

    public VariableValue(int value)
    {
      IntegerValue = value;
    }

    public VariableValue(long value)
    {
      LongValue = value;
    }

    public VariableValue(float value)
    {
      FloatValue = value;
    }

    public VariableValue(double value)
    {
      DoubleValue = value;
    }

    public VariableValue(decimal value)
    {
      DecimalValue = value;
    }

    public VariableValue(bool value)
    {
      BooleanValue = value;
    }

    public VariableValue(string value)
    {
      StringValue = value;
    }

    public void CopyFrom(VariableValue other)
    {
      _type = other._type;
      // Only copy the active union member based on type
      switch (other._type)
      {
        case Type.Integer:
        case Type.Long:
        case Type.Boolean:
          _value.IntValue =
            other._value.IntValue; // Copy all 8 bytes of union (boolean uses 1 byte, but copying 8 is faster)
          break;
        case Type.Float:
        case Type.Double:
          _value.FloatValue = other._value.FloatValue;
          break;
        case Type.Decimal:
          _decimalValue = other._decimalValue;
          break;
        case Type.String:
          _stringValue = other._stringValue;
          break;
      }
    }
  }
}