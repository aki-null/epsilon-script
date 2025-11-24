using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EpsilonScript.AST.Literal;
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

    // Location context for runtime error reporting
    internal SourceLocation Location = SourceLocation.Unknown;

    /// <summary>
    /// Creates a RuntimeException with this node's location context.
    /// </summary>
    protected RuntimeException CreateRuntimeException(string message)
    {
      return new RuntimeException(message, Location);
    }

    public string StringValue
    {
      get
      {
        switch (_type)
        {
          case ExtendedType.String:
            return _stringValue;
          case ExtendedType.Integer:
          case ExtendedType.Long:
            return _value.IntValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
          case ExtendedType.Float:
            return ((float)_value.FloatValue).ToString(System.Globalization.CultureInfo.InvariantCulture);
          case ExtendedType.Double:
            return _value.FloatValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
          case ExtendedType.Decimal:
            return _decimalValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
          case ExtendedType.Boolean:
            return (_value.IntValue != 0).ToString();
          case ExtendedType.Null:
          case ExtendedType.Undefined:
            return null;
          default:
            throw new InvalidOperationException($"Cannot convert {_type} to string");
        }
      }
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
    public virtual bool IsPrecomputable => true;

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
            throw new InvalidOperationException($"Cannot convert {_type} to integer");
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
            throw new InvalidOperationException($"Cannot convert {_type} to long");
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
            throw new InvalidOperationException($"Cannot convert {_type} to float");
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
            throw new InvalidOperationException($"Cannot convert {_type} to double");
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
            throw new InvalidOperationException($"Cannot convert {_type} to decimal");
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
            throw new InvalidOperationException($"Cannot convert {_type} to boolean");
        }
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      protected set
      {
        _type = ExtendedType.Boolean;
        _value.IntValue = value ? 1L : 0L;
      }
    }

    /// <summary>
    /// Template method that automatically captures location and delegates to BuildCore().
    /// Subclasses should override BuildCore() instead of this method.
    /// </summary>
    public void Build(Stack<Node> rpnStack, Element element, CompilerContext context,
      Compiler.Options options, IVariableContainer variables)
    {
      Location = element.Token.Location;
      BuildCore(rpnStack, element, context, options, variables);
    }

    /// <summary>
    /// Core build implementation. Override this method to implement node-specific construction logic.
    /// Location is automatically captured before this method is called.
    /// </summary>
    protected abstract void BuildCore(Stack<Node> rpnStack, Element element, CompilerContext context,
      Compiler.Options options, IVariableContainer variables);

    public virtual void Execute(IVariableContainer variablesOverride)
    {
    }

    // IMPORTANT: When overriding Optimize(), always follow these rules:
    // 1. Call child.Optimize() before returning or using children
    //    WRONG: return _childNode;
    //    RIGHT: return _childNode.Optimize();
    // 2. Execute precomputable nodes before reading their values
    //    WRONG: if (node.IsPrecomputable && node.BooleanValue) ...
    //    RIGHT: if (node.IsPrecomputable) { node.Execute(null); if (node.BooleanValue) ... }
    // 3. Common pattern: if (IsPrecomputable) { Execute(null); return CreateValueNode(); }
    public virtual Node Optimize()
    {
      return this;
    }

    /// <summary>
    /// Validates the AST node after optimization completes.
    /// Called after optimization phase when types are fully resolved for constant expressions.
    /// Override in nodes that need post-optimization validation (e.g., function signature checking).
    /// </summary>
    public virtual void Validate()
    {
      // Base implementation does nothing - leaf nodes don't need validation
    }

    /// <summary>
    /// Configures NoAlloc validation on this node and all child nodes recursively.
    /// Called after optimization phase completes to enable runtime allocation checking.
    /// </summary>
    public virtual void ConfigureNoAlloc()
    {
      // Base implementation does nothing - leaf nodes don't need validation
    }

    protected Node CreateValueNode()
    {
      Node node = ValueType switch
      {
        ExtendedType.Integer => new IntegerNode(IntegerValue),
        ExtendedType.Long => new IntegerNode(LongValue),
        ExtendedType.Float => new FloatNode(FloatValue),
        ExtendedType.Double => new FloatNode(DoubleValue),
        ExtendedType.Decimal => new FloatNode(DecimalValue),
        ExtendedType.Boolean => new BooleanNode(BooleanValue),
        ExtendedType.String => new StringNode(StringValue),
        _ => throw new ArgumentOutOfRangeException(nameof(ValueType), ValueType, "Unsupported value type")
      };

      // Preserve location information from the original node
      node.Location = Location;
      return node;
    }
  }
}