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

    public override bool IsConstant => _leftNode.IsConstant && _rightNode.IsConstant;

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IVariableContainer variables,
      IDictionary<uint, CustomFunctionOverload> functions)
    {
      ValueType = ValueType.Boolean;
      _comparisonType = element.Type;

      if (!rpnStack.TryPop(out _rightNode) || !rpnStack.TryPop(out _leftNode))
      {
        throw new ParserException(element.Token, "Cannot find values to perform comparison operation on");
      }
    }

    private bool EqualTo()
    {
      switch (_comparisonValueType)
      {
        case ValueType.Integer:
          return _leftNode.IntegerValue == _rightNode.IntegerValue;
        case ValueType.Float:
          return Math.IsNearlyEqual(_leftNode.FloatValue, _rightNode.FloatValue);
        case ValueType.Boolean:
          return _leftNode.BooleanValue == _rightNode.BooleanValue;
        case ValueType.String:
          return string.Equals(_leftNode.StringValue, _rightNode.StringValue, StringComparison.Ordinal);
        default:
          throw new ArgumentOutOfRangeException(nameof(_comparisonValueType), _comparisonValueType,
            "Unsupported comparison value type");
      }
    }

    private bool LessThan()
    {
      switch (_comparisonValueType)
      {
        case ValueType.Integer:
          return _leftNode.IntegerValue < _rightNode.IntegerValue;
        case ValueType.Float:
          return _leftNode.FloatValue < _rightNode.FloatValue;
        default:
          throw new ArgumentOutOfRangeException(nameof(_comparisonValueType), _comparisonValueType,
            "Unsupported comparison value type");
      }
    }

    private bool GreaterThan()
    {
      switch (_comparisonValueType)
      {
        case ValueType.Integer:
          return _leftNode.IntegerValue > _rightNode.IntegerValue;
        case ValueType.Float:
          return _leftNode.FloatValue > _rightNode.FloatValue;
        default:
          throw new ArgumentOutOfRangeException(nameof(_comparisonValueType), _comparisonValueType,
            "Unsupported comparison value type");
      }
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

      if (_leftNode.ValueType == ValueType.String)
      {
        if (_rightNode.ValueType != ValueType.String)
        {
          throw new RuntimeException("String can only be compared against a string");
        }

        _comparisonValueType = ValueType.String;
      }
      else if (_leftNode.ValueType == ValueType.Boolean)
      {
        _comparisonValueType = ValueType.Boolean;
      }
      else if (_leftNode.ValueType == ValueType.Float || _rightNode.ValueType == ValueType.Float)
      {
        _comparisonValueType = ValueType.Float;
      }
      else
      {
        _comparisonValueType = ValueType.Integer;
      }

      switch (_comparisonType)
      {
        case ElementType.ComparisonEqual:
          BooleanValue = EqualTo();
          break;
        case ElementType.ComparisonNotEqual:
          BooleanValue = !EqualTo();
          break;
        case ElementType.ComparisonLessThan:
          BooleanValue = LessThan();
          break;
        case ElementType.ComparisonGreaterThan:
          BooleanValue = GreaterThan();
          break;
        case ElementType.ComparisonLessThanOrEqualTo:
          BooleanValue = LessThan() || EqualTo();
          break;
        case ElementType.ComparisonGreaterThanOrEqualTo:
          BooleanValue = GreaterThan() || EqualTo();
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(_comparisonType), _comparisonType,
            "Unsupported comparison type");
      }

      IntegerValue = BooleanValue ? 1 : 0;
      FloatValue = IntegerValue;
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