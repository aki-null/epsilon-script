using System;
using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  public class ComparisonNode : Node
  {
    private Node _leftNode;
    private Node _rightNode;
    private ElementType _comparisonType;
    private ValueType _comparisonValueType;
    private ValueType _configuredIntegerType;
    private ValueType _configuredFloatType;

    public override bool IsConstant => _leftNode.IsConstant && _rightNode.IsConstant;

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IVariableContainer variables,
      IDictionary<VariableId, CustomFunctionOverload> functions,
      Compiler.IntegerPrecision intPrecision, Compiler.FloatPrecision floatPrecision)
    {
      ValueType = ValueType.Boolean;
      _comparisonType = element.Type;

      if (!rpnStack.TryPop(out _rightNode) || !rpnStack.TryPop(out _leftNode))
      {
        throw new ParserException(element.Token, "Cannot find values to perform comparison operation on");
      }

      // Store configured types for optimized type promotion
      _configuredIntegerType = intPrecision == Compiler.IntegerPrecision.Integer
        ? ValueType.Integer
        : ValueType.Long;
      _configuredFloatType = floatPrecision switch
      {
        Compiler.FloatPrecision.Float => ValueType.Float,
        Compiler.FloatPrecision.Double => ValueType.Double,
        Compiler.FloatPrecision.Decimal => ValueType.Decimal,
        _ => ValueType.Float
      };
    }

    private ValueType PromoteType(ValueType left, ValueType right)
    {
      if (left == ValueType.String || right == ValueType.String)
        return ValueType.String;

      if (left == ValueType.Boolean || right == ValueType.Boolean)
        return ValueType.Boolean;

      // Since precision is fixed per compiler, only two numeric types exist:
      // the configured integer type and the configured float type
      if (left == _configuredFloatType || right == _configuredFloatType)
        return _configuredFloatType;

      return _configuredIntegerType;
    }

    public override void Execute(IVariableContainer variablesOverride)
    {
      _leftNode.Execute(variablesOverride);
      _rightNode.Execute(variablesOverride);

      switch (_comparisonType)
      {
        case ElementType.ComparisonEqual:
        case ElementType.ComparisonNotEqual:
          if (_leftNode.ValueType == ValueType.Tuple || _rightNode.ValueType == ValueType.Tuple ||
              _leftNode.IsNumeric != _rightNode.IsNumeric)
          {
            throw new RuntimeException("Cannot perform comparison for different value types");
          }

          break;
        case ElementType.ComparisonLessThan:
        case ElementType.ComparisonGreaterThan:
        case ElementType.ComparisonLessThanOrEqualTo:
        case ElementType.ComparisonGreaterThanOrEqualTo:
          if (!_leftNode.IsNumeric || !_rightNode.IsNumeric)
          {
            throw new RuntimeException("Cannot find values to perform arithmetic comparision on non numeric types");
          }

          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(_comparisonType), _comparisonType,
            "Unsupported comparision type");
      }

      if (_leftNode.ValueType == ValueType.String && _rightNode.ValueType != ValueType.String)
      {
        throw new RuntimeException("String can only be compared against a string");
      }

      _comparisonValueType = PromoteType(_leftNode.ValueType, _rightNode.ValueType);

      BooleanValue = (_comparisonType, _comparisonValueType) switch
      {
        // Equal comparisons
        (ElementType.ComparisonEqual, ValueType.Integer) =>
          _leftNode.IntegerValue == _rightNode.IntegerValue,
        (ElementType.ComparisonEqual, ValueType.Long) =>
          _leftNode.LongValue == _rightNode.LongValue,
        (ElementType.ComparisonEqual, ValueType.Float) =>
          Math.IsNearlyEqual(_leftNode.FloatValue, _rightNode.FloatValue),
        (ElementType.ComparisonEqual, ValueType.Double) =>
          Math.IsNearlyEqual(_leftNode.DoubleValue, _rightNode.DoubleValue),
        (ElementType.ComparisonEqual, ValueType.Decimal) =>
          _leftNode.DecimalValue == _rightNode.DecimalValue,
        (ElementType.ComparisonEqual, ValueType.Boolean) =>
          _leftNode.BooleanValue == _rightNode.BooleanValue,
        (ElementType.ComparisonEqual, ValueType.String) =>
          string.Equals(_leftNode.StringValue, _rightNode.StringValue, StringComparison.Ordinal),

        // Not equal comparisons
        (ElementType.ComparisonNotEqual, ValueType.Integer) =>
          _leftNode.IntegerValue != _rightNode.IntegerValue,
        (ElementType.ComparisonNotEqual, ValueType.Long) =>
          _leftNode.LongValue != _rightNode.LongValue,
        (ElementType.ComparisonNotEqual, ValueType.Float) =>
          !Math.IsNearlyEqual(_leftNode.FloatValue, _rightNode.FloatValue),
        (ElementType.ComparisonNotEqual, ValueType.Double) =>
          !Math.IsNearlyEqual(_leftNode.DoubleValue, _rightNode.DoubleValue),
        (ElementType.ComparisonNotEqual, ValueType.Decimal) =>
          _leftNode.DecimalValue != _rightNode.DecimalValue,
        (ElementType.ComparisonNotEqual, ValueType.Boolean) =>
          _leftNode.BooleanValue != _rightNode.BooleanValue,
        (ElementType.ComparisonNotEqual, ValueType.String) =>
          !string.Equals(_leftNode.StringValue, _rightNode.StringValue, StringComparison.Ordinal),

        // Less than comparisons
        (ElementType.ComparisonLessThan, ValueType.Integer) =>
          _leftNode.IntegerValue < _rightNode.IntegerValue,
        (ElementType.ComparisonLessThan, ValueType.Long) =>
          _leftNode.LongValue < _rightNode.LongValue,
        (ElementType.ComparisonLessThan, ValueType.Float) =>
          _leftNode.FloatValue < _rightNode.FloatValue,
        (ElementType.ComparisonLessThan, ValueType.Double) =>
          _leftNode.DoubleValue < _rightNode.DoubleValue,
        (ElementType.ComparisonLessThan, ValueType.Decimal) =>
          _leftNode.DecimalValue < _rightNode.DecimalValue,

        // Less than or equal comparisons
        (ElementType.ComparisonLessThanOrEqualTo, ValueType.Integer) =>
          _leftNode.IntegerValue <= _rightNode.IntegerValue,
        (ElementType.ComparisonLessThanOrEqualTo, ValueType.Long) =>
          _leftNode.LongValue <= _rightNode.LongValue,
        (ElementType.ComparisonLessThanOrEqualTo, ValueType.Float) =>
          _leftNode.FloatValue <= _rightNode.FloatValue,
        (ElementType.ComparisonLessThanOrEqualTo, ValueType.Double) =>
          _leftNode.DoubleValue <= _rightNode.DoubleValue,
        (ElementType.ComparisonLessThanOrEqualTo, ValueType.Decimal) =>
          _leftNode.DecimalValue <= _rightNode.DecimalValue,

        // Greater than comparisons
        (ElementType.ComparisonGreaterThan, ValueType.Integer) =>
          _leftNode.IntegerValue > _rightNode.IntegerValue,
        (ElementType.ComparisonGreaterThan, ValueType.Long) =>
          _leftNode.LongValue > _rightNode.LongValue,
        (ElementType.ComparisonGreaterThan, ValueType.Float) =>
          _leftNode.FloatValue > _rightNode.FloatValue,
        (ElementType.ComparisonGreaterThan, ValueType.Double) =>
          _leftNode.DoubleValue > _rightNode.DoubleValue,
        (ElementType.ComparisonGreaterThan, ValueType.Decimal) =>
          _leftNode.DecimalValue > _rightNode.DecimalValue,

        // Greater than or equal comparisons
        (ElementType.ComparisonGreaterThanOrEqualTo, ValueType.Integer) =>
          _leftNode.IntegerValue >= _rightNode.IntegerValue,
        (ElementType.ComparisonGreaterThanOrEqualTo, ValueType.Long) =>
          _leftNode.LongValue >= _rightNode.LongValue,
        (ElementType.ComparisonGreaterThanOrEqualTo, ValueType.Float) =>
          _leftNode.FloatValue >= _rightNode.FloatValue,
        (ElementType.ComparisonGreaterThanOrEqualTo, ValueType.Double) =>
          _leftNode.DoubleValue >= _rightNode.DoubleValue,
        (ElementType.ComparisonGreaterThanOrEqualTo, ValueType.Decimal) =>
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