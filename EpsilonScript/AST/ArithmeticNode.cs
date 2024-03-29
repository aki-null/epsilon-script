using System;
using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  public class ArithmeticNode : Node
  {
    private Node _leftNode;
    private Node _rightNode;
    private ElementType _operator;

    public override bool IsConstant => _leftNode.IsConstant && _rightNode.IsConstant;

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IVariableContainer variables, IDictionary<uint, CustomFunctionOverload> functions)
    {
      if (!rpnStack.TryPop(out _rightNode) || !rpnStack.TryPop(out _leftNode))
      {
        throw new ParserException(element.Token, "Cannot find values to perform arithmetic operation on");
      }

      _operator = element.Type;
    }

    private int CalculateIntegerValue()
    {
      switch (_operator)
      {
        case ElementType.AddOperator:
          return _leftNode.IntegerValue + _rightNode.IntegerValue;
        case ElementType.SubtractOperator:
          return _leftNode.IntegerValue - _rightNode.IntegerValue;
        case ElementType.MultiplyOperator:
          return _leftNode.IntegerValue * _rightNode.IntegerValue;
        case ElementType.DivideOperator:
          return _leftNode.IntegerValue / _rightNode.IntegerValue;
        case ElementType.ModuloOperator:
          return _leftNode.IntegerValue % _rightNode.IntegerValue;
        default:
          throw new ArgumentOutOfRangeException(nameof(_operator), _operator, "Unsupported operator type");
      }
    }

    private float CalculateFloatValue()
    {
      switch (_operator)
      {
        case ElementType.AddOperator:
          return _leftNode.FloatValue + _rightNode.FloatValue;
        case ElementType.SubtractOperator:
          return _leftNode.FloatValue - _rightNode.FloatValue;
        case ElementType.MultiplyOperator:
          return _leftNode.FloatValue * _rightNode.FloatValue;
        case ElementType.DivideOperator:
          return _leftNode.FloatValue / _rightNode.FloatValue;
        case ElementType.ModuloOperator:
          return _leftNode.FloatValue % _rightNode.FloatValue;
        default:
          throw new ArgumentOutOfRangeException(nameof(_operator), _operator, "Unsupported operator type");
      }
    }

    private string CalculateStringValue()
    {
      switch (_operator)
      {
        case ElementType.AddOperator:
          // The left node is guaranteed to be a string node
          return _leftNode.StringValue + _rightNode.ToString();
        default:
          throw new ArgumentOutOfRangeException(nameof(_operator), _operator, "Unsupported operator type");
      }
    }

    public override void Execute(IVariableContainer variablesOverride)
    {
      _leftNode.Execute(variablesOverride);
      _rightNode.Execute(variablesOverride);

      if ((!_leftNode.IsNumeric || !_rightNode.IsNumeric) && _leftNode.ValueType != ValueType.String)
      {
        throw new RuntimeException("An arithmetic operation can only be performed on numeric values");
      }

      switch (_leftNode.ValueType)
      {
        case ValueType.Integer:
          ValueType = _rightNode.ValueType;
          break;
        case ValueType.Float:
          ValueType = ValueType.Float;
          break;
        case ValueType.String:
          ValueType = ValueType.String;
          break;
        default:
          ValueType = ValueType;
          break;
      }

      switch (ValueType)
      {
        case ValueType.Integer:
          IntegerValue = CalculateIntegerValue();
          FloatValue = IntegerValue;
          BooleanValue = IntegerValue != 0;
          break;
        case ValueType.Float:
          FloatValue = CalculateFloatValue();
          IntegerValue = (int)FloatValue;
          BooleanValue = IntegerValue != 0;
          break;
        case ValueType.String:
          StringValue = CalculateStringValue();
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(ValueType), ValueType, "Unsupported value type");
      }
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