using System;
using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  internal class ComparisonNode : Node
  {
    private Node _leftNode;
    private Node _rightNode;
    private ElementType _comparisonType;
    private ExtendedType _comparisonValueType;
    private Type _configuredIntegerType;
    private Type _configuredFloatType;

    public override bool IsConstant => _leftNode.IsConstant && _rightNode.IsConstant;

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IVariableContainer variables,
      IDictionary<VariableId, CustomFunctionOverload> functions,
      Compiler.IntegerPrecision intPrecision, Compiler.FloatPrecision floatPrecision)
    {
      ValueType = ExtendedType.Boolean;
      _comparisonType = element.Type;

      if (!rpnStack.TryPop(out _rightNode) || !rpnStack.TryPop(out _leftNode))
      {
        throw new ParserException(element.Token, "Cannot find values to perform comparison operation on");
      }

      // Store configured types for optimized type promotion
      _configuredIntegerType = intPrecision == Compiler.IntegerPrecision.Integer
        ? Type.Integer
        : Type.Long;
      _configuredFloatType = floatPrecision switch
      {
        Compiler.FloatPrecision.Float => Type.Float,
        Compiler.FloatPrecision.Double => Type.Double,
        Compiler.FloatPrecision.Decimal => Type.Decimal,
        _ => Type.Float
      };
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
      if (left == (ExtendedType)_configuredFloatType || right == (ExtendedType)_configuredFloatType)
        return (ExtendedType)_configuredFloatType;

      return (ExtendedType)_configuredIntegerType;
    }

    public override void Execute(IVariableContainer variablesOverride)
    {
      _leftNode.Execute(variablesOverride);
      _rightNode.Execute(variablesOverride);

      switch (_comparisonType)
      {
        case ElementType.ComparisonEqual:
        case ElementType.ComparisonNotEqual:
          if (_leftNode.ValueType == ExtendedType.Tuple || _rightNode.ValueType == ExtendedType.Tuple)
          {
            throw new RuntimeException(
              $"Cannot perform comparison on tuple types (left: {_leftNode.ValueType}, right: {_rightNode.ValueType})");
          }

          // Check for type compatibility: numeric types can only be compared with other numeric types
          if (_leftNode.IsNumeric != _rightNode.IsNumeric)
          {
            throw new RuntimeException(
              $"Cannot compare incompatible types: {_leftNode.ValueType} and {_rightNode.ValueType} (numeric types can only be compared with other numeric types)");
          }

          // Additional check for non-numeric types: String can only be compared with String
          // (Boolean can be compared with Boolean, both are non-numeric, so first check allows it)
          if (_leftNode.ValueType == ExtendedType.String && _rightNode.ValueType != ExtendedType.String)
          {
            throw new RuntimeException("String can only be compared against a string");
          }

          break;
        case ElementType.ComparisonLessThan:
        case ElementType.ComparisonGreaterThan:
        case ElementType.ComparisonLessThanOrEqualTo:
        case ElementType.ComparisonGreaterThanOrEqualTo:
          if (!_leftNode.IsNumeric || !_rightNode.IsNumeric)
          {
            throw new RuntimeException(
              $"Cannot perform arithmetic comparison on non-numeric types (left: {_leftNode.ValueType}, right: {_rightNode.ValueType})");
          }

          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(_comparisonType), _comparisonType,
            "Unsupported comparison type");
      }

      _comparisonValueType = DetermineComparisonType(_leftNode.ValueType, _rightNode.ValueType);

      // Perform comparison based on operator and promoted type.
      // Note: Float/Double use fuzzy equality (IsNearlyEqual) to handle precision errors.
      // All other types (Integer, Long, Decimal, Boolean, String) use exact equality.
      BooleanValue = (_comparisonType, _comparisonValueType) switch
      {
        // Equal comparisons
        (ElementType.ComparisonEqual, ExtendedType.Integer) =>
          _leftNode.IntegerValue == _rightNode.IntegerValue,
        (ElementType.ComparisonEqual, ExtendedType.Long) =>
          _leftNode.LongValue == _rightNode.LongValue,
        (ElementType.ComparisonEqual, ExtendedType.Float) =>
          Math.IsNearlyEqual(_leftNode.FloatValue, _rightNode.FloatValue),
        (ElementType.ComparisonEqual, ExtendedType.Double) =>
          Math.IsNearlyEqual(_leftNode.DoubleValue, _rightNode.DoubleValue),
        (ElementType.ComparisonEqual, ExtendedType.Decimal) =>
          _leftNode.DecimalValue == _rightNode.DecimalValue,
        (ElementType.ComparisonEqual, ExtendedType.Boolean) =>
          _leftNode.BooleanValue == _rightNode.BooleanValue,
        (ElementType.ComparisonEqual, ExtendedType.String) =>
          string.Equals(_leftNode.StringValue, _rightNode.StringValue, StringComparison.Ordinal),

        // Not equal comparisons
        (ElementType.ComparisonNotEqual, ExtendedType.Integer) =>
          _leftNode.IntegerValue != _rightNode.IntegerValue,
        (ElementType.ComparisonNotEqual, ExtendedType.Long) =>
          _leftNode.LongValue != _rightNode.LongValue,
        (ElementType.ComparisonNotEqual, ExtendedType.Float) =>
          !Math.IsNearlyEqual(_leftNode.FloatValue, _rightNode.FloatValue),
        (ElementType.ComparisonNotEqual, ExtendedType.Double) =>
          !Math.IsNearlyEqual(_leftNode.DoubleValue, _rightNode.DoubleValue),
        (ElementType.ComparisonNotEqual, ExtendedType.Decimal) =>
          _leftNode.DecimalValue != _rightNode.DecimalValue,
        (ElementType.ComparisonNotEqual, ExtendedType.Boolean) =>
          _leftNode.BooleanValue != _rightNode.BooleanValue,
        (ElementType.ComparisonNotEqual, ExtendedType.String) =>
          !string.Equals(_leftNode.StringValue, _rightNode.StringValue, StringComparison.Ordinal),

        // Less than comparisons
        (ElementType.ComparisonLessThan, ExtendedType.Integer) =>
          _leftNode.IntegerValue < _rightNode.IntegerValue,
        (ElementType.ComparisonLessThan, ExtendedType.Long) =>
          _leftNode.LongValue < _rightNode.LongValue,
        (ElementType.ComparisonLessThan, ExtendedType.Float) =>
          _leftNode.FloatValue < _rightNode.FloatValue,
        (ElementType.ComparisonLessThan, ExtendedType.Double) =>
          _leftNode.DoubleValue < _rightNode.DoubleValue,
        (ElementType.ComparisonLessThan, ExtendedType.Decimal) =>
          _leftNode.DecimalValue < _rightNode.DecimalValue,

        // Less than or equal comparisons
        (ElementType.ComparisonLessThanOrEqualTo, ExtendedType.Integer) =>
          _leftNode.IntegerValue <= _rightNode.IntegerValue,
        (ElementType.ComparisonLessThanOrEqualTo, ExtendedType.Long) =>
          _leftNode.LongValue <= _rightNode.LongValue,
        (ElementType.ComparisonLessThanOrEqualTo, ExtendedType.Float) =>
          _leftNode.FloatValue <= _rightNode.FloatValue,
        (ElementType.ComparisonLessThanOrEqualTo, ExtendedType.Double) =>
          _leftNode.DoubleValue <= _rightNode.DoubleValue,
        (ElementType.ComparisonLessThanOrEqualTo, ExtendedType.Decimal) =>
          _leftNode.DecimalValue <= _rightNode.DecimalValue,

        // Greater than comparisons
        (ElementType.ComparisonGreaterThan, ExtendedType.Integer) =>
          _leftNode.IntegerValue > _rightNode.IntegerValue,
        (ElementType.ComparisonGreaterThan, ExtendedType.Long) =>
          _leftNode.LongValue > _rightNode.LongValue,
        (ElementType.ComparisonGreaterThan, ExtendedType.Float) =>
          _leftNode.FloatValue > _rightNode.FloatValue,
        (ElementType.ComparisonGreaterThan, ExtendedType.Double) =>
          _leftNode.DoubleValue > _rightNode.DoubleValue,
        (ElementType.ComparisonGreaterThan, ExtendedType.Decimal) =>
          _leftNode.DecimalValue > _rightNode.DecimalValue,

        // Greater than or equal comparisons
        (ElementType.ComparisonGreaterThanOrEqualTo, ExtendedType.Integer) =>
          _leftNode.IntegerValue >= _rightNode.IntegerValue,
        (ElementType.ComparisonGreaterThanOrEqualTo, ExtendedType.Long) =>
          _leftNode.LongValue >= _rightNode.LongValue,
        (ElementType.ComparisonGreaterThanOrEqualTo, ExtendedType.Float) =>
          _leftNode.FloatValue >= _rightNode.FloatValue,
        (ElementType.ComparisonGreaterThanOrEqualTo, ExtendedType.Double) =>
          _leftNode.DoubleValue >= _rightNode.DoubleValue,
        (ElementType.ComparisonGreaterThanOrEqualTo, ExtendedType.Decimal) =>
          _leftNode.DecimalValue >= _rightNode.DecimalValue,

        _ => throw new ArgumentOutOfRangeException(
          nameof(_comparisonType),
          $"Unsupported comparison: {_comparisonType} on {_comparisonValueType}")
      };
    }

    public override Node Optimize()
    {
      if (IsConstant)
      {
        Execute(null);
        return CreateValueNode();
      }

      _leftNode = _leftNode.Optimize();
      _rightNode = _rightNode.Optimize();
      return this;
    }
  }
}