using System;
using System.Collections.Generic;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST.Arithmetic
{
  /// <summary>
  /// Abstract base class for multiply-add/subtract operations.
  /// Computes (a * b) ± c in a single operation to reduce Execute() calls.
  /// Concrete subclasses implement specific operation patterns.
  /// </summary>
  internal abstract class MultiplyAddOperationNode : Node
  {
    protected Node Multiplier1; // a in (a * b) ± c
    protected Node Multiplier2; // b in (a * b) ± c
    protected Node Addend; // c in (a * b) ± c
    private readonly ExtendedType _configuredIntegerType;
    private readonly ExtendedType _configuredFloatType;
    protected bool NoAllocMode;
    protected readonly CompilerContext Context;

    // Secondary location - for the multiply operation
    // Primary location (base.Location) stores the add/subtract operation
    private readonly SourceLocation _multiplyLocation;

    public override bool IsPrecomputable =>
      Multiplier1.IsPrecomputable && Multiplier2.IsPrecomputable && Addend.IsPrecomputable;

    /// <summary>
    /// Constructor used during optimization phase to create multiply-add node.
    /// </summary>
    protected MultiplyAddOperationNode(Node multiplier1, Node multiplier2, Node addend,
      MultiplyNode multiplyNode, ArithmeticOperationNode addSubtractNode, CompilerContext context)
    {
      Multiplier1 = multiplier1;
      Multiplier2 = multiplier2;
      Addend = addend;
      Context = context;
      _configuredIntegerType = context.ConfiguredIntegerType;
      _configuredFloatType = context.ConfiguredFloatType;

      // Primary location: the add/subtract operation (top-level operation)
      Location = addSubtractNode.Location;

      // Secondary location: the multiply operation (nested operation)
      _multiplyLocation = multiplyNode.Location;
    }

    protected override void BuildCore(Stack<Node> rpnStack, Element element, CompilerContext context,
      Compiler.Options options, IVariableContainer variables)
    {
      // This node is only created during optimization, never during initial AST build
      throw new InvalidOperationException($"{GetType().Name} cannot be built from RPN stack");
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    protected ExtendedType PromoteType(ExtendedType left, ExtendedType right)
    {
      if (left == ExtendedType.String || right == ExtendedType.String)
        return ExtendedType.String;

      if (left == _configuredFloatType || right == _configuredFloatType)
        return _configuredFloatType;

      return _configuredIntegerType;
    }

    /// <summary>
    /// Returns true if this operation supports string operands (string concatenation).
    /// Only AddMultiplyNode returns true.
    /// </summary>
    protected virtual bool SupportsStrings() => false;

    /// <summary>
    /// Returns the error message for when strings are used with this operation.
    /// </summary>
    protected virtual string GetStringErrorMessage()
    {
      return "An arithmetic operation can only be performed on numeric values";
    }

    /// <summary>
    /// Calculates the string result for this operation.
    /// Only called when SupportsStrings() returns true.
    /// </summary>
    protected virtual void CalculateString(ExtendedType multiplyType)
    {
      throw new RuntimeException("String operations only support concatenation with string on left side", Location);
    }

    public override void Execute(IVariableContainer variablesOverride)
    {
      // Execute all operands once
      Multiplier1.Execute(variablesOverride);
      Multiplier2.Execute(variablesOverride);
      Addend.Execute(variablesOverride);

      // Check for boolean operands upfront
      if (Multiplier1.ValueType == ExtendedType.Boolean || Multiplier2.ValueType == ExtendedType.Boolean)
      {
        throw new RuntimeException("Boolean values cannot be used in arithmetic operations", _multiplyLocation);
      }

      if (Addend.ValueType == ExtendedType.Boolean)
      {
        throw new RuntimeException("Boolean values cannot be used in arithmetic operations", Location);
      }

      // Validate types - matches original ArithmeticNode behavior
      // Multiply must be numeric * numeric (strings not supported in multiply)
      if (!Multiplier1.IsNumeric || !Multiplier2.IsNumeric)
      {
        throw new RuntimeException("Multiply operation can only be performed on numeric values", _multiplyLocation);
      }

      // For add/subtract: addend can be string only if operation supports it
      if (!Addend.IsNumeric && Addend.ValueType != ExtendedType.String)
      {
        throw new RuntimeException("An arithmetic operation can only be performed on numeric values", Location);
      }

      // String validation: check if this operation supports strings
      if (Addend.ValueType == ExtendedType.String)
      {
        if (!SupportsStrings())
        {
          throw new RuntimeException(GetStringErrorMessage(), Location);
        }
      }

      // Determine result type: promote through multiply first, then add
      var multiplyType = PromoteType(Multiplier1.ValueType, Multiplier2.ValueType);
      ValueType = PromoteType(multiplyType, Addend.ValueType);

      // Perform multiply-add/subtract operation
      switch (ValueType)
      {
        case ExtendedType.Integer:
          CalculateInteger();
          break;
        case ExtendedType.Long:
          CalculateLong();
          break;
        case ExtendedType.Float:
          CalculateFloat();
          break;
        case ExtendedType.Double:
          CalculateDouble();
          break;
        case ExtendedType.Decimal:
          CalculateDecimal();
          break;
        case ExtendedType.String:
          CalculateString(multiplyType);
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(ValueType), ValueType, "Unsupported value type");
      }
    }

    /// <summary>
    /// Performs the multiply-add/subtract operation on integer values.
    /// </summary>
    protected abstract void CalculateInteger();

    /// <summary>
    /// Performs the multiply-add/subtract operation on long values.
    /// </summary>
    protected abstract void CalculateLong();

    /// <summary>
    /// Performs the multiply-add/subtract operation on float values.
    /// </summary>
    protected abstract void CalculateFloat();

    /// <summary>
    /// Performs the multiply-add/subtract operation on double values.
    /// </summary>
    protected abstract void CalculateDouble();

    /// <summary>
    /// Performs the multiply-add/subtract operation on decimal values.
    /// </summary>
    protected abstract void CalculateDecimal();

    public override Node Optimize()
    {
      if (IsPrecomputable)
      {
        Execute(null);
        return CreateValueNode();
      }

      Multiplier1 = Multiplier1.Optimize();
      Multiplier2 = Multiplier2.Optimize();
      Addend = Addend.Optimize();

      return this;
    }

    public override void Validate()
    {
      Multiplier1?.Validate();
      Multiplier2?.Validate();
      Addend?.Validate();
    }

    public override void ConfigureNoAlloc()
    {
      NoAllocMode = true;
      Multiplier1?.ConfigureNoAlloc();
      Multiplier2?.ConfigureNoAlloc();
      Addend?.ConfigureNoAlloc();
    }
  }
}