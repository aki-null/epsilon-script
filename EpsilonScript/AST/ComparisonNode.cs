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
    private Type _comparisonValueType;
    private Type _configuredIntegerType;
    private Type _configuredFloatType;

    public override bool IsConstant => _leftNode.IsConstant && _rightNode.IsConstant;

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IVariableContainer variables,
      IDictionary<VariableId, CustomFunctionOverload> functions,
      Compiler.IntegerPrecision intPrecision, Compiler.FloatPrecision floatPrecision)
    {
      ValueType = Type.Boolean;
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
    private Type PromoteType(Type left, Type right)
    {
      if (left == Type.String || right == Type.String)
        return Type.String;

      if (left == Type.Boolean || right == Type.Boolean)
        return Type.Boolean;

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
          if (_leftNode.ValueType == Type.Tuple || _rightNode.ValueType == Type.Tuple ||
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

      if (_leftNode.ValueType == Type.String && _rightNode.ValueType != Type.String)
      {
        throw new RuntimeException("String can only be compared against a string");
      }

      _comparisonValueType = PromoteType(_leftNode.ValueType, _rightNode.ValueType);

      BooleanValue = (_comparisonType, _comparisonValueType) switch
      {
        // Equal comparisons
        (ElementType.ComparisonEqual, Type.Integer) =>
          _leftNode.IntegerValue == _rightNode.IntegerValue,
        (ElementType.ComparisonEqual, Type.Long) =>
          _leftNode.LongValue == _rightNode.LongValue,
        (ElementType.ComparisonEqual, Type.Float) =>
          Math.IsNearlyEqual(_leftNode.FloatValue, _rightNode.FloatValue),
        (ElementType.ComparisonEqual, Type.Double) =>
          Math.IsNearlyEqual(_leftNode.DoubleValue, _rightNode.DoubleValue),
        (ElementType.ComparisonEqual, Type.Decimal) =>
          _leftNode.DecimalValue == _rightNode.DecimalValue,
        (ElementType.ComparisonEqual, Type.Boolean) =>
          _leftNode.BooleanValue == _rightNode.BooleanValue,
        (ElementType.ComparisonEqual, Type.String) =>
          string.Equals(_leftNode.StringValue, _rightNode.StringValue, StringComparison.Ordinal),

        // Not equal comparisons
        (ElementType.ComparisonNotEqual, Type.Integer) =>
          _leftNode.IntegerValue != _rightNode.IntegerValue,
        (ElementType.ComparisonNotEqual, Type.Long) =>
          _leftNode.LongValue != _rightNode.LongValue,
        (ElementType.ComparisonNotEqual, Type.Float) =>
          !Math.IsNearlyEqual(_leftNode.FloatValue, _rightNode.FloatValue),
        (ElementType.ComparisonNotEqual, Type.Double) =>
          !Math.IsNearlyEqual(_leftNode.DoubleValue, _rightNode.DoubleValue),
        (ElementType.ComparisonNotEqual, Type.Decimal) =>
          _leftNode.DecimalValue != _rightNode.DecimalValue,
        (ElementType.ComparisonNotEqual, Type.Boolean) =>
          _leftNode.BooleanValue != _rightNode.BooleanValue,
        (ElementType.ComparisonNotEqual, Type.String) =>
          !string.Equals(_leftNode.StringValue, _rightNode.StringValue, StringComparison.Ordinal),

        // Less than comparisons
        (ElementType.ComparisonLessThan, Type.Integer) =>
          _leftNode.IntegerValue < _rightNode.IntegerValue,
        (ElementType.ComparisonLessThan, Type.Long) =>
          _leftNode.LongValue < _rightNode.LongValue,
        (ElementType.ComparisonLessThan, Type.Float) =>
          _leftNode.FloatValue < _rightNode.FloatValue,
        (ElementType.ComparisonLessThan, Type.Double) =>
          _leftNode.DoubleValue < _rightNode.DoubleValue,
        (ElementType.ComparisonLessThan, Type.Decimal) =>
          _leftNode.DecimalValue < _rightNode.DecimalValue,

        // Less than or equal comparisons
        (ElementType.ComparisonLessThanOrEqualTo, Type.Integer) =>
          _leftNode.IntegerValue <= _rightNode.IntegerValue,
        (ElementType.ComparisonLessThanOrEqualTo, Type.Long) =>
          _leftNode.LongValue <= _rightNode.LongValue,
        (ElementType.ComparisonLessThanOrEqualTo, Type.Float) =>
          _leftNode.FloatValue <= _rightNode.FloatValue,
        (ElementType.ComparisonLessThanOrEqualTo, Type.Double) =>
          _leftNode.DoubleValue <= _rightNode.DoubleValue,
        (ElementType.ComparisonLessThanOrEqualTo, Type.Decimal) =>
          _leftNode.DecimalValue <= _rightNode.DecimalValue,

        // Greater than comparisons
        (ElementType.ComparisonGreaterThan, Type.Integer) =>
          _leftNode.IntegerValue > _rightNode.IntegerValue,
        (ElementType.ComparisonGreaterThan, Type.Long) =>
          _leftNode.LongValue > _rightNode.LongValue,
        (ElementType.ComparisonGreaterThan, Type.Float) =>
          _leftNode.FloatValue > _rightNode.FloatValue,
        (ElementType.ComparisonGreaterThan, Type.Double) =>
          _leftNode.DoubleValue > _rightNode.DoubleValue,
        (ElementType.ComparisonGreaterThan, Type.Decimal) =>
          _leftNode.DecimalValue > _rightNode.DecimalValue,

        // Greater than or equal comparisons
        (ElementType.ComparisonGreaterThanOrEqualTo, Type.Integer) =>
          _leftNode.IntegerValue >= _rightNode.IntegerValue,
        (ElementType.ComparisonGreaterThanOrEqualTo, Type.Long) =>
          _leftNode.LongValue >= _rightNode.LongValue,
        (ElementType.ComparisonGreaterThanOrEqualTo, Type.Float) =>
          _leftNode.FloatValue >= _rightNode.FloatValue,
        (ElementType.ComparisonGreaterThanOrEqualTo, Type.Double) =>
          _leftNode.DoubleValue >= _rightNode.DoubleValue,
        (ElementType.ComparisonGreaterThanOrEqualTo, Type.Decimal) =>
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