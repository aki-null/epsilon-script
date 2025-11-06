using System;
using System.Collections.Generic;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST.Comparison
{
  /// <summary>
  /// Abstract base class for all comparison operation nodes.
  /// Provides common infrastructure for comparison operations.
  /// Concrete subclasses implement operation-specific comparison logic.
  /// </summary>
  internal abstract class ComparisonOperationNode : Node
  {
    protected Node LeftNode;
    protected Node RightNode;
    private ExtendedType _configuredIntegerType;
    private ExtendedType _configuredFloatType;

    public override bool IsPrecomputable => LeftNode.IsPrecomputable && RightNode.IsPrecomputable;

    /// <summary>
    /// Returns true if this comparison supports non-numeric types (Boolean, String).
    /// Equality comparisons return true; ordering comparisons return false.
    /// </summary>
    protected virtual bool SupportsNonNumericTypes() => false;

    protected override void BuildCore(Stack<Node> rpnStack, Element element, CompilerContext context,
      Compiler.Options options, IVariableContainer variables)
    {
      ValueType = ExtendedType.Boolean;

      if (!rpnStack.TryPop(out RightNode) || !rpnStack.TryPop(out LeftNode))
      {
        throw new ParserException(element.Token, "Cannot find values to perform comparison operation on");
      }

      // Store configured types for optimized type promotion
      _configuredIntegerType = context.ConfiguredIntegerType;
      _configuredFloatType = context.ConfiguredFloatType;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private ExtendedType DetermineComparisonType(ExtendedType left, ExtendedType right)
    {
      if (left == ExtendedType.String || right == ExtendedType.String)
        return ExtendedType.String;

      if (left == ExtendedType.Boolean || right == ExtendedType.Boolean)
        return ExtendedType.Boolean;

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

      // Validation specific to comparison type
      ValidateOperands();

      var comparisonValueType = DetermineComparisonType(LeftNode.ValueType, RightNode.ValueType);

      // Perform type-specific comparison
      BooleanValue = CompareValues(comparisonValueType);
    }

    /// <summary>
    /// Validates that operands are compatible for this comparison operation.
    /// Equality comparisons validate type compatibility; ordering comparisons require numeric types.
    /// </summary>
    private void ValidateOperands()
    {
      if (SupportsNonNumericTypes())
      {
        // Equality comparison validation
        var leftType = LeftNode.ValueType;
        var rightType = RightNode.ValueType;

        // Check for tuple types (not comparable)
        if (leftType == ExtendedType.Tuple || rightType == ExtendedType.Tuple)
        {
          throw CreateRuntimeException(
            $"Cannot perform comparison on tuple types (left: {leftType}, right: {rightType})");
        }

        // Valid comparisons: both numeric, or exact same non-numeric type (String==String, Boolean==Boolean)
        if (leftType != rightType)
        {
          // Types differ - only valid if both are numeric (allows Integer==Float, etc.)
          var leftIsNumeric = LeftNode.IsNumeric;
          var rightIsNumeric = RightNode.IsNumeric;

          if (!leftIsNumeric || !rightIsNumeric)
          {
            // Provide detailed error message based on which type is non-numeric
            if (leftIsNumeric || rightIsNumeric)
            {
              // One is numeric, one is not
              throw CreateRuntimeException(
                $"Cannot compare incompatible types: {leftType} and {rightType} (numeric types can only be compared with other numeric types)");
            }

            // Both are non-numeric but different (e.g., Boolean vs String)
            throw CreateRuntimeException($"Cannot compare incompatible types: {leftType} and {rightType}");
          }
        }
      }
      else
      {
        // Ordering comparison validation - requires numeric types
        if (!LeftNode.IsNumeric || !RightNode.IsNumeric)
        {
          throw CreateRuntimeException(
            $"Cannot perform arithmetic comparison on non-numeric types (left: {LeftNode.ValueType}, right: {RightNode.ValueType})");
        }
      }
    }

    /// <summary>
    /// Performs the comparison based on the promoted value type.
    /// Each concrete class implements its specific comparison logic.
    /// </summary>
    private bool CompareValues(ExtendedType comparisonType)
    {
      return comparisonType switch
      {
        ExtendedType.Integer => CompareInteger(),
        ExtendedType.Long => CompareLong(),
        ExtendedType.Float => CompareFloat(),
        ExtendedType.Double => CompareDouble(),
        ExtendedType.Decimal => CompareDecimal(),
        ExtendedType.Boolean => CompareBoolean(),
        ExtendedType.String => CompareString(),
        _ => throw new ArgumentOutOfRangeException(nameof(comparisonType), comparisonType,
          $"Unsupported type for comparison")
      };
    }

    protected abstract bool CompareInteger();
    protected abstract bool CompareLong();
    protected abstract bool CompareFloat();
    protected abstract bool CompareDouble();
    protected abstract bool CompareDecimal();

    /// <summary>
    /// Compares boolean values. Only equality operations support this.
    /// Default throws exception; equality operations override.
    /// </summary>
    protected virtual bool CompareBoolean()
    {
      throw new ArgumentOutOfRangeException(nameof(ExtendedType.Boolean),
        "Boolean comparison not supported for this operation");
    }

    /// <summary>
    /// Compares string values. Only equality operations support this.
    /// Default throws exception; equality operations override.
    /// </summary>
    protected virtual bool CompareString()
    {
      throw new ArgumentOutOfRangeException(nameof(ExtendedType.String),
        "String comparison not supported for this operation");
    }

    public override Node Optimize()
    {
      if (IsPrecomputable)
      {
        Execute(null);
        return CreateValueNode();
      }

      LeftNode = LeftNode.Optimize();
      RightNode = RightNode.Optimize();
      return this;
    }

    public override void Validate()
    {
      LeftNode?.Validate();
      RightNode?.Validate();
    }

    public override void ConfigureNoAlloc()
    {
      LeftNode?.ConfigureNoAlloc();
      RightNode?.ConfigureNoAlloc();
    }
  }
}