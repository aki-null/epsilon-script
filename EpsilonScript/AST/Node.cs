using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  internal abstract class Node
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

    private ExtendedType _type;
    private ValueUnion _value;
    private decimal _decimalValue;
    private string _stringValue;

    public string StringValue
    {
      get => _stringValue;
      protected set
      {
        _type = ExtendedType.String;
        _stringValue = value;
      }
    }

    public List<Node> TupleValue { get; protected set; }

    public VariableValue Variable { get; protected set; }

    internal ExtendedType ValueType
    {
      get => _type;
      set => _type = value;
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
          case ExtendedType.Integer:
          case ExtendedType.Long:
            return (int)_value.IntValue;
          case ExtendedType.Float:
          case ExtendedType.Double:
            var floatVal = _value.FloatValue;
            if (double.IsNaN(floatVal) || double.IsInfinity(floatVal))
              return 0;
            return (int)floatVal;
          case ExtendedType.Decimal:
            return (int)_decimalValue;
          case ExtendedType.Boolean:
            return _value.IntValue != 0 ? 1 : 0;
          case ExtendedType.String:
          case ExtendedType.Null:
          case ExtendedType.Undefined:
            return 0;
          default:
            throw new RuntimeException($"Cannot convert {_type} to integer");
        }
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      protected set
      {
        _type = ExtendedType.Integer;
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
          case ExtendedType.Integer:
          case ExtendedType.Long:
            return _value.IntValue;
          case ExtendedType.Float:
          case ExtendedType.Double:
            var floatVal = _value.FloatValue;
            if (double.IsNaN(floatVal) || double.IsInfinity(floatVal))
              return 0;
            return (long)floatVal;
          case ExtendedType.Decimal:
            return (long)_decimalValue;
          case ExtendedType.Boolean:
            return _value.IntValue;
          case ExtendedType.String:
          case ExtendedType.Null:
          case ExtendedType.Undefined:
            return 0L;
          default:
            throw new RuntimeException($"Cannot convert {_type} to long");
        }
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      protected set
      {
        _type = ExtendedType.Long;
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
          case ExtendedType.Integer:
          case ExtendedType.Long:
            return _value.IntValue;
          case ExtendedType.Float:
          case ExtendedType.Double:
            return (float)_value.FloatValue;
          case ExtendedType.Decimal:
            return (float)_decimalValue;
          case ExtendedType.Boolean:
            return _value.IntValue != 0 ? 1.0f : 0.0f;
          case ExtendedType.String:
          case ExtendedType.Null:
          case ExtendedType.Undefined:
            return 0.0f;
          default:
            throw new RuntimeException($"Cannot convert {_type} to float");
        }
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      protected set
      {
        _type = ExtendedType.Float;
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
          case ExtendedType.Integer:
          case ExtendedType.Long:
            return _value.IntValue;
          case ExtendedType.Float:
          case ExtendedType.Double:
            return _value.FloatValue;
          case ExtendedType.Decimal:
            return (double)_decimalValue;
          case ExtendedType.Boolean:
            return _value.IntValue != 0 ? 1.0 : 0.0;
          case ExtendedType.String:
          case ExtendedType.Null:
          case ExtendedType.Undefined:
            return 0.0;
          default:
            throw new RuntimeException($"Cannot convert {_type} to double");
        }
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      protected set
      {
        _type = ExtendedType.Double;
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
          case ExtendedType.Integer:
          case ExtendedType.Long:
            return _value.IntValue;
          case ExtendedType.Float:
          case ExtendedType.Double:
            return (decimal)_value.FloatValue;
          case ExtendedType.Decimal:
            return _decimalValue;
          case ExtendedType.Boolean:
            return _value.IntValue != 0 ? 1m : 0m;
          case ExtendedType.String:
          case ExtendedType.Null:
          case ExtendedType.Undefined:
            return 0m;
          default:
            throw new RuntimeException($"Cannot convert {_type} to decimal");
        }
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      protected set
      {
        _type = ExtendedType.Decimal;
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
          case ExtendedType.Boolean:
          case ExtendedType.Integer:
          case ExtendedType.Long:
            return _value.IntValue != 0;
          case ExtendedType.Float:
          case ExtendedType.Double:
            var val = _value.FloatValue;
            return val != 0.0 && !double.IsNaN(val);
          case ExtendedType.Decimal:
            return _decimalValue != 0m;
          case ExtendedType.String:
          case ExtendedType.Null:
          case ExtendedType.Undefined:
            return false;
          default:
            throw new RuntimeException($"Cannot convert {_type} to boolean");
        }
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      protected set
      {
        _type = ExtendedType.Boolean;
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
        case ExtendedType.Integer:
          return new IntegerNode(IntegerValue);
        case ExtendedType.Long:
          return new IntegerNode(LongValue);
        case ExtendedType.Float:
          return new FloatNode(FloatValue);
        case ExtendedType.Double:
          return new FloatNode(DoubleValue);
        case ExtendedType.Decimal:
          return new FloatNode(DecimalValue);
        case ExtendedType.Boolean:
          return new BooleanNode(BooleanValue);
        case ExtendedType.String:
          return new StringNode(StringValue);
        default:
          throw new ArgumentOutOfRangeException(nameof(ValueType), ValueType, "Unsupported value type");
      }
    }

    public override string ToString()
    {
      switch (ValueType)
      {
        case ExtendedType.Undefined:
          return "Undefined";
        case ExtendedType.Null:
          return "null";
        case ExtendedType.Integer:
          return IntegerValue.ToString();
        case ExtendedType.Long:
          return LongValue.ToString();
        case ExtendedType.Float:
          return FloatValue.ToString(CultureInfo.InvariantCulture);
        case ExtendedType.Double:
          return DoubleValue.ToString(CultureInfo.InvariantCulture);
        case ExtendedType.Decimal:
          return DecimalValue.ToString(CultureInfo.InvariantCulture);
        case ExtendedType.Boolean:
          return BooleanValue ? "true" : "false";
        case ExtendedType.Tuple:
          return "Tuple";
        case ExtendedType.String:
          return StringValue;
        default:
          throw new ArgumentOutOfRangeException(nameof(ValueType), ValueType, "Unsupported value type");
      }
    }
  }
}