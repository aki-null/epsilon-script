using System;
using System.Collections.Generic;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST.Arithmetic
{
  /// <summary>
  /// Abstract base class for all arithmetic operation nodes.
  /// Provides common infrastructure for binary arithmetic operations (Add, Subtract, Multiply, Divide, Modulo).
  /// Concrete subclasses implement operation-specific calculation logic.
  /// </summary>
  internal abstract class ArithmeticOperationNode : Node
  {
    protected Node LeftNode;
    protected Node RightNode;
    private ExtendedType _configuredIntegerType;
    private ExtendedType _configuredFloatType;
    private bool _noAllocMode;
    protected CompilerContext Context;

    public override bool IsPrecomputable => LeftNode.IsPrecomputable && RightNode.IsPrecomputable;

    /// <summary>
    /// Returns the human-readable name of this operation for error messages.
    /// Examples: "addition", "multiplication", "division"
    /// </summary>
    protected abstract string GetOperatorName();

    /// <summary>
    /// Calculates the string result for this operation.
    /// Only called when SupportsStrings() returns true and a string operand is detected.
    /// Default implementation throws an exception; AddNode overrides this.
    /// </summary>
    protected virtual void CalculateString()
    {
      throw CreateRuntimeException(
        $"String operations only support concatenation (+), not {GetOperatorName()}");
    }

    protected override void BuildCore(Stack<Node> rpnStack, Element element, CompilerContext context,
      Compiler.Options options, IVariableContainer variables)
    {
      if (!rpnStack.TryPop(out RightNode) || !rpnStack.TryPop(out LeftNode))
      {
        throw new ParserException(element.Token, "Cannot find values to perform arithmetic operation on");
      }

      Context = context;

      // Store configured types for optimized type promotion
      _configuredIntegerType = context.ConfiguredIntegerType;
      _configuredFloatType = context.ConfiguredFloatType;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private ExtendedType PromoteType(ExtendedType left, ExtendedType right)
    {
      if (left == ExtendedType.String || right == ExtendedType.String)
        return ExtendedType.String;

      // Since precision is fixed per compiler, only two numeric types exist:
      // the configured integer type and the configured float type
      if (left == _configuredFloatType || right == _configuredFloatType)
        return _configuredFloatType;

      return _configuredIntegerType;
    }

    public override void Execute(IVariableContainer variablesOverride)
    {
      LeftNode.Execute(variablesOverride);
      RightNode.Execute(variablesOverride);

      ValueType = PromoteType(LeftNode.ValueType, RightNode.ValueType);

      if ((!LeftNode.IsNumeric || !RightNode.IsNumeric) && LeftNode.ValueType != ExtendedType.String)
      {
        if (LeftNode.ValueType == ExtendedType.Boolean || RightNode.ValueType == ExtendedType.Boolean)
        {
          throw CreateRuntimeException(
            $"Boolean values cannot be used in arithmetic operations ({GetOperatorName()})");
        }

        throw CreateRuntimeException("An arithmetic operation can only be performed on numeric values");
      }

      // NoAlloc validation: Block runtime string concatenation
      if (_noAllocMode && ValueType == ExtendedType.String)
      {
        throw CreateRuntimeException(
          "String concatenation is not allowed in NoAlloc mode (causes runtime heap allocation)");
      }

      var targetType = ValueType;
      switch (targetType)
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
          CalculateString();
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(targetType), targetType, "Unsupported value type");
      }
    }

    /// <summary>
    /// Performs the arithmetic operation on integer values.
    /// Reads operands from _leftNode.IntegerValue and _rightNode.IntegerValue.
    /// Stores result in IntegerValue property.
    /// </summary>
    protected abstract void CalculateInteger();

    /// <summary>
    /// Performs the arithmetic operation on long values.
    /// Reads operands from _leftNode.LongValue and _rightNode.LongValue.
    /// Stores result in LongValue property.
    /// </summary>
    protected abstract void CalculateLong();

    /// <summary>
    /// Performs the arithmetic operation on float values.
    /// Reads operands from _leftNode.FloatValue and _rightNode.FloatValue.
    /// Stores result in FloatValue property.
    /// </summary>
    protected abstract void CalculateFloat();

    /// <summary>
    /// Performs the arithmetic operation on double values.
    /// Reads operands from _leftNode.DoubleValue and _rightNode.DoubleValue.
    /// Stores result in DoubleValue property.
    /// </summary>
    protected abstract void CalculateDouble();

    /// <summary>
    /// Performs the arithmetic operation on decimal values.
    /// Reads operands from _leftNode.DecimalValue and _rightNode.DecimalValue.
    /// Stores result in DecimalValue property.
    /// </summary>
    protected abstract void CalculateDecimal();

    public override void Validate()
    {
      LeftNode?.Validate();
      RightNode?.Validate();
    }

    public override void ConfigureNoAlloc()
    {
      _noAllocMode = true;
      LeftNode?.ConfigureNoAlloc();
      RightNode?.ConfigureNoAlloc();
    }
  }
}