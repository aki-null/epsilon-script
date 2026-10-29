using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System;
using EpsilonScript.AST.Assignment;
using EpsilonScript.Intermediate;
using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST
{
  internal abstract class Node
  {
    /// Value storage using union approach for memory efficiency
    /// - Union stores long/double/decimal (16 bytes, overlapping; none hold references)
    /// - Type field indicates which value is semantically valid
    /// - Conversions happen on-demand during read operations
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
    private struct ValueUnion
    {
      [System.Runtime.InteropServices.FieldOffset(0)]
      public long IntValue;

      [System.Runtime.InteropServices.FieldOffset(0)]
      public double FloatValue;

      [System.Runtime.InteropServices.FieldOffset(0)]
      public decimal DecimalValue;
    }

    private ExtendedType _type;
    private ValueUnion _value;
    private string _stringValue;

    // Location context for runtime error reporting
    internal SourceLocation Location = SourceLocation.Unknown;

    // Set by Build; nodes created during optimization inherit it. Null for nodes constructed directly.
    internal CompilerContext Context;

    // The pool that created this node; null for nodes constructed directly
    internal NodePool Pool;
    internal NodeKind PoolKind;

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
            return _value.DecimalValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
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

    // Only tuple nodes carry this; virtual keeps the field out of every other node.
    public virtual List<Node> TupleValue => null;

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
            return (int)_value.DecimalValue;
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
            return (long)_value.DecimalValue;
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
            return (float)_value.DecimalValue;
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
            return (double)_value.DecimalValue;
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
            return _value.DecimalValue;
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
        _value.DecimalValue = value;
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
            return _value.DecimalValue != 0m;
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
      Context = context;
      BuildCore(rpnStack, element, context, options, variables);
    }

    /// <summary>
    /// Core build implementation. Override this method to implement node-specific construction logic.
    /// Location is automatically captured before this method is called.
    /// </summary>
    protected abstract void BuildCore(Stack<Node> rpnStack, Element element, CompilerContext context,
      Compiler.Options options, IVariableContainer variables);

    /// <summary>
    /// Evaluates this subtree on the register VM, the only implementation of the language semantics, and keeps
    /// the result as this node's value. Constant folding and tests use it; compiled scripts run their own program.
    /// </summary>
    internal void Execute(IVariableContainer variablesOverride) => Evaluate(variablesOverride, false);

    /// <summary>
    /// Execute for a subtree that stays in the tree: every executed descendant also keeps its value, as the tree
    /// interpreter left one in each node it ran, and the subtree's later Optimize calls read them. A folded subtree
    /// is replaced by its value node, so plain Execute skips that work.
    /// </summary>
    internal void ExecuteInPlace() => Evaluate(null, true);

    private void Evaluate(IVariableContainer variablesOverride, bool inPlace)
    {
      // Only nodes constructed directly, rather than built from source, have no context
      var context = Context ?? new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float,
        null);
      context.Evaluate(this, variablesOverride, inPlace);
    }

    /// <summary>
    /// Keeps the value this node computed in an evaluated program as its value.
    /// </summary>
    internal void SetResult(RegisterProgram program, int register)
    {
      _type = program.ReadType(register);
      switch (_type)
      {
        case ExtendedType.String:
          _stringValue = program.ReadString(register);
          break;
        case ExtendedType.Decimal:
          _value.DecimalValue = program.ReadDecimal(register);
          break;
        default:
          _value.IntValue = program.ReadRawBits(register);
          break;
      }
    }

    // Raw access for seeding VM registers; the integer and binary float members of the union share the same 8 bytes
    internal long RawBits => _value.IntValue;

    /// <summary>
    /// Emits this node into a register program and returns its result register.
    /// </summary>
    internal virtual int Lower(RegisterCompiler compiler) =>
      throw new InvalidOperationException($"{GetType().Name} cannot be lowered");

    /// <summary>
    /// Emits an assignment of value to this node. Only variables can be assigned; the VM reports the error for
    /// anything else when the assignment runs.
    /// </summary>
    internal virtual int LowerAssignmentTarget(RegisterCompiler compiler, AssignmentOperationNode assignment,
      Node value) => compiler.LowerAssignNonVariable(assignment, this, value);

    // Multiply-add forms report multiply errors at the multiply operator
    internal virtual SourceLocation MultiplyLocation => Location;

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
    /// Restores the state of a newly constructed node so the pool can build it again.
    /// Overrides clear their own fields and call the base implementation.
    /// </summary>
    internal virtual void Reset()
    {
      _type = ExtendedType.Undefined;
      _value = default;
      _stringValue = null;
      Location = SourceLocation.Unknown;
      Context = null;
    }

    /// <summary>
    /// Creates a node during optimization, from this node's pool when it has one.
    /// </summary>
    protected Node RentNode(NodeKind kind)
    {
      var node = Pool != null ? Pool.Rent(kind) : NodePool.Create(kind);
      node.Context = Context;
      return node;
    }

    protected Node CreateBooleanNode(bool value)
    {
      var node = RentNode(NodeKind.Boolean);
      node.BooleanValue = value;
      return node;
    }

    protected Node CreateValueNode()
    {
      Node node;
      switch (ValueType)
      {
        case ExtendedType.Integer:
          node = RentNode(NodeKind.Integer);
          node.IntegerValue = IntegerValue;
          break;
        case ExtendedType.Long:
          node = RentNode(NodeKind.Integer);
          node.LongValue = LongValue;
          break;
        case ExtendedType.Float:
          node = RentNode(NodeKind.Float);
          node.FloatValue = FloatValue;
          break;
        case ExtendedType.Double:
          node = RentNode(NodeKind.Float);
          node.DoubleValue = DoubleValue;
          break;
        case ExtendedType.Decimal:
          node = RentNode(NodeKind.Float);
          node.DecimalValue = DecimalValue;
          break;
        case ExtendedType.Boolean:
          node = RentNode(NodeKind.Boolean);
          node.BooleanValue = BooleanValue;
          break;
        case ExtendedType.String:
          node = RentNode(NodeKind.String);
          node.StringValue = StringValue;
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(ValueType), ValueType, "Unsupported value type");
      }

      // Preserve location information from the original node
      node.Location = Location;
      return node;
    }
  }
}