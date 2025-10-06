using System;
using System.Runtime.InteropServices;
using System.Globalization;

namespace EpsilonScript
{
  [StructLayout(LayoutKind.Explicit)]
  public class VariableValue
  {
    // Offset 0: Type tag (4 bytes)
    [FieldOffset(0)] private Type _type;

    // Offset 8: Union of 8-byte numeric types (8 bytes total, 8-byte aligned)
    [FieldOffset(8)] private long _intValue; // For Integer and Long
    [FieldOffset(8)] private double _floatValue; // For Float and Double
    [FieldOffset(8)] private bool _boolValue; // For Boolean (1 byte)

    // Offset 16: Decimal (16 bytes, 8-byte aligned)
    [FieldOffset(16)] private decimal _decimalValue;

    // Offset 32: String reference (8 bytes on 64-bit, 8-byte aligned)
    [FieldOffset(32)] private string _stringValue;

    public Type Type => _type;

    public int IntegerValue
    {
      get
      {
        switch (_type)
        {
          case Type.Integer:
          case Type.Long:
            return (int)_intValue;
          case Type.Float:
          case Type.Double:
            if (double.IsNaN(_floatValue) || double.IsInfinity(_floatValue))
              return 0;
            return (int)_floatValue;
          case Type.Decimal:
            return (int)_decimalValue;
          case Type.Boolean:
            return _boolValue ? 1 : 0;
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
            _intValue = value;
            break;
          case Type.Integer:
          case Type.Long:
            _intValue = value;
            break;
          case Type.Float:
          case Type.Double:
            _floatValue = value;
            break;
          case Type.Decimal:
            _decimalValue = value;
            break;
          case Type.Boolean:
            _boolValue = value != 0;
            break;
          case Type.String:
            _stringValue = value.ToString();
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
            return _intValue;
          case Type.Float:
          case Type.Double:
            if (double.IsNaN(_floatValue) || double.IsInfinity(_floatValue))
              return 0;
            return (long)_floatValue;
          case Type.Decimal:
            return (long)_decimalValue;
          case Type.Boolean:
            return _boolValue ? 1L : 0L;
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
            _intValue = value;
            break;
          case Type.Integer:
          case Type.Long:
            _intValue = value;
            break;
          case Type.Float:
          case Type.Double:
            _floatValue = value;
            break;
          case Type.Decimal:
            _decimalValue = value;
            break;
          case Type.Boolean:
            _boolValue = value != 0;
            break;
          case Type.String:
            _stringValue = value.ToString();
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
            return _intValue;
          case Type.Float:
          case Type.Double:
            return (float)_floatValue;
          case Type.Decimal:
            return (float)_decimalValue;
          case Type.Boolean:
            throw new InvalidCastException("A boolean value cannot be casted to a float value");
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
            _floatValue = value;
            break;
          case Type.Integer:
          case Type.Long:
            _intValue = (long)value;
            break;
          case Type.Float:
          case Type.Double:
            _floatValue = value;
            break;
          case Type.Decimal:
            _decimalValue = (decimal)value;
            break;
          case Type.Boolean:
            throw new InvalidCastException("A float value cannot be casted to a boolean value");
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
            return _intValue;
          case Type.Float:
          case Type.Double:
            return _floatValue;
          case Type.Decimal:
            return (double)_decimalValue;
          case Type.Boolean:
            throw new InvalidCastException("A boolean value cannot be casted to a double value");
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
            _floatValue = value;
            break;
          case Type.Integer:
          case Type.Long:
            _intValue = (long)value;
            break;
          case Type.Float:
          case Type.Double:
            _floatValue = value;
            break;
          case Type.Decimal:
            _decimalValue = (decimal)value;
            break;
          case Type.Boolean:
            throw new InvalidCastException("A double value cannot be casted to a boolean value");
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
            return _intValue;
          case Type.Float:
          case Type.Double:
            return (decimal)_floatValue;
          case Type.Decimal:
            return _decimalValue;
          case Type.Boolean:
            throw new InvalidCastException("A boolean value cannot be casted to a decimal value");
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
            _intValue = (long)value;
            break;
          case Type.Float:
          case Type.Double:
            _floatValue = (double)value;
            break;
          case Type.Decimal:
            _decimalValue = value;
            break;
          case Type.Boolean:
            throw new InvalidCastException("A decimal value cannot be casted to a boolean value");
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
            return _boolValue;
          case Type.Integer:
          case Type.Long:
            return _intValue != 0;
          case Type.Float:
          case Type.Double:
            throw new InvalidCastException($"Cannot convert {_type} to boolean");
          case Type.Decimal:
            throw new InvalidCastException($"Cannot convert {_type} to boolean");
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
            _boolValue = value;
            break;
          case Type.Integer:
          case Type.Long:
            _intValue = value ? 1 : 0;
            break;
          case Type.Boolean:
            _boolValue = value;
            break;
          case Type.Float:
          case Type.Double:
            throw new InvalidCastException("A boolean value cannot be casted to a float value");
          case Type.Decimal:
            throw new InvalidCastException("A boolean value cannot be casted to a decimal value");
          default:
            throw new InvalidCastException($"Cannot assign boolean to {_type}");
        }
      }
    }

    public string StringValue
    {
      get => _stringValue;
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
          _intValue = other._intValue; // Boolean stored in same union, copy entire 8 bytes
          break;
        case Type.Float:
        case Type.Double:
          _floatValue = other._floatValue;
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