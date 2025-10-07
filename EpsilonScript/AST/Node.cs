using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  public abstract class Node
  {
    /// Value storage using union approach for memory efficiency
    /// - Union stores long/double (8 bytes, overlapping)
    /// - Decimal stored separately (16 bytes, cannot overlap safely)
    /// - Type field indicates which value is semantically valid
    /// - Conversions happen on-demand during read operations
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
    private struct ValueUnion
    {
      [System.Runtime.InteropServices.FieldOffset(0)]
      public long IntValue;

      [System.Runtime.InteropServices.FieldOffset(0)]
      public double FloatValue;
    }

    private Type _type;
    private ValueUnion _value;
    private decimal _decimalValue;
    private string _stringValue;

    public string StringValue
    {
      get => _stringValue;
      protected set
      {
        _type = Type.String;
        _stringValue = value;
      }
    }

    public List<Node> TupleValue { get; protected set; }

    public VariableValue Variable { get; protected set; }

    public Type ValueType
    {
      get => _type;
      protected set => _type = value;
    }

    public bool IsNumeric => _type.IsNumber();
    public virtual bool IsConstant => true;

    public int IntegerValue
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get
      {
        switch (_type)
        {
          case Type.Integer:
          case Type.Long:
            return (int)_value.IntValue;
          case Type.Float:
          case Type.Double:
            var floatVal = _value.FloatValue;
            if (double.IsNaN(floatVal) || double.IsInfinity(floatVal))
              return 0;
            return (int)floatVal;
          case Type.Decimal:
            return (int)_decimalValue;
          case Type.Boolean:
            return _value.IntValue != 0 ? 1 : 0;
          case Type.String:
          case Type.Null:
          case Type.Undefined:
            return 0;
          default:
            throw new RuntimeException($"Cannot convert {_type} to integer");
        }
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      protected set
      {
        _type = Type.Integer;
        _value.IntValue = value;
      }
    }

    public long LongValue
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get
      {
        switch (_type)
        {
          case Type.Integer:
          case Type.Long:
            return _value.IntValue;
          case Type.Float:
          case Type.Double:
            var floatVal = _value.FloatValue;
            if (double.IsNaN(floatVal) || double.IsInfinity(floatVal))
              return 0;
            return (long)floatVal;
          case Type.Decimal:
            return (long)_decimalValue;
          case Type.Boolean:
            return _value.IntValue;
          case Type.String:
          case Type.Null:
          case Type.Undefined:
            return 0L;
          default:
            throw new RuntimeException($"Cannot convert {_type} to long");
        }
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      protected set
      {
        _type = Type.Long;
        _value.IntValue = value;
      }
    }

    public float FloatValue
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            return _value.IntValue != 0 ? 1.0f : 0.0f;
          case Type.String:
          case Type.Null:
          case Type.Undefined:
            return 0.0f;
          default:
            throw new RuntimeException($"Cannot convert {_type} to float");
        }
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      protected set
      {
        _type = Type.Float;
        _value.FloatValue = value;
      }
    }

    public double DoubleValue
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            return _value.IntValue != 0 ? 1.0 : 0.0;
          case Type.String:
          case Type.Null:
          case Type.Undefined:
            return 0.0;
          default:
            throw new RuntimeException($"Cannot convert {_type} to double");
        }
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      protected set
      {
        _type = Type.Double;
        _value.FloatValue = value;
      }
    }

    public decimal DecimalValue
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            return _value.IntValue != 0 ? 1m : 0m;
          case Type.String:
          case Type.Null:
          case Type.Undefined:
            return 0m;
          default:
            throw new RuntimeException($"Cannot convert {_type} to decimal");
        }
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      protected set
      {
        _type = Type.Decimal;
        _decimalValue = value;
      }
    }

    public bool BooleanValue
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get
      {
        switch (_type)
        {
          case Type.Boolean:
          case Type.Integer:
          case Type.Long:
            return _value.IntValue != 0;
          case Type.Float:
          case Type.Double:
            var val = _value.FloatValue;
            return val != 0.0 && !double.IsNaN(val);
          case Type.Decimal:
            return _decimalValue != 0m;
          case Type.String:
          case Type.Null:
          case Type.Undefined:
            return false;
          default:
            throw new RuntimeException($"Cannot convert {_type} to boolean");
        }
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      protected set
      {
        _type = Type.Boolean;
        _value.IntValue = value ? 1L : 0L;
      }
    }

    public abstract void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IVariableContainer variables, IDictionary<VariableId, CustomFunctionOverload> functions,
      Compiler.IntegerPrecision intPrecision, Compiler.FloatPrecision floatPrecision);

    public virtual void Execute(IVariableContainer variablesOverride)
    {
    }

    // IMPORTANT: When overriding Optimize(), always follow these rules:
    // 1. Call child.Optimize() before returning or using children
    //    WRONG: return _childNode;
    //    RIGHT: return _childNode.Optimize();
    // 2. Execute constant nodes before reading their values
    //    WRONG: if (node.IsConstant && node.BooleanValue) ...
    //    RIGHT: if (node.IsConstant) { node.Execute(null); if (node.BooleanValue) ... }
    // 3. Common pattern: if (IsConstant) { Execute(null); return CreateValueNode(); }
    public virtual Node Optimize()
    {
      return this;
    }

    public Node CreateValueNode()
    {
      switch (ValueType)
      {
        case Type.Integer:
          return new IntegerNode(IntegerValue);
        case Type.Long:
          return new IntegerNode(LongValue);
        case Type.Float:
          return new FloatNode(FloatValue);
        case Type.Double:
          return new FloatNode(DoubleValue);
        case Type.Decimal:
          return new FloatNode(DecimalValue);
        case Type.Boolean:
          return new BooleanNode(BooleanValue);
        case Type.String:
          return new StringNode(StringValue);
        default:
          throw new ArgumentOutOfRangeException(nameof(ValueType), ValueType, "Unsupported value type");
      }
    }

    public override string ToString()
    {
      switch (ValueType)
      {
        case Type.Undefined:
          return "Undefined";
        case Type.Null:
          return "null";
        case Type.Integer:
          return IntegerValue.ToString();
        case Type.Long:
          return LongValue.ToString();
        case Type.Float:
          return FloatValue.ToString(CultureInfo.InvariantCulture);
        case Type.Double:
          return DoubleValue.ToString(CultureInfo.InvariantCulture);
        case Type.Decimal:
          return DecimalValue.ToString(CultureInfo.InvariantCulture);
        case Type.Boolean:
          return BooleanValue ? "true" : "false";
        case Type.Tuple:
          return "Tuple";
        case Type.String:
          return StringValue;
        default:
          throw new ArgumentOutOfRangeException(nameof(ValueType), ValueType, "Unsupported value type");
      }
    }
  }
}