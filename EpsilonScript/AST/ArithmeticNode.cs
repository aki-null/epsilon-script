using System;
using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Parser;

namespace EpsilonScript.AST
{
  public class ArithmeticNode : Node
  {
    private Node _leftNode;
    private Node _rightNode;
    private ElementType _operator;

    public override bool IsConstant => _leftNode.IsConstant && _rightNode.IsConstant;

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IDictionary<string, VariableValue> variables,
      IDictionary<string, CustomFunctionOverload> functions)
    {
      if (!rpnStack.TryPop(out _rightNode) || !rpnStack.TryPop(out _leftNode))
      {
        throw new ParserException(element.Token, "Cannot find values to perform arithmetic operation on");
      }

      _operator = element.Type;
    }

    private int CalculateIntegerValue()
    {
      return _operator switch
      {
        ElementType.AddOperator => (_leftNode.IntegerValue + _rightNode.IntegerValue),
        ElementType.SubtractOperator => (_leftNode.IntegerValue - _rightNode.IntegerValue),
        ElementType.MultiplyOperator => (_leftNode.IntegerValue * _rightNode.IntegerValue),
        ElementType.DivideOperator => (_leftNode.IntegerValue / _rightNode.IntegerValue),
        _ => throw new ArgumentOutOfRangeException(nameof(_operator), _operator, "Unsupported operator type")
      };
    }

    private float CalculateFloatValue()
    {
      return _operator switch
      {
        ElementType.AddOperator => (_leftNode.FloatValue + _rightNode.FloatValue),
        ElementType.SubtractOperator => (_leftNode.FloatValue - _rightNode.FloatValue),
        ElementType.MultiplyOperator => (_leftNode.FloatValue * _rightNode.FloatValue),
        ElementType.DivideOperator => (_leftNode.FloatValue / _rightNode.FloatValue),
        _ => throw new ArgumentOutOfRangeException(nameof(_operator), _operator, "Unsupported operator type")
      };
    }

    public override void Execute()
    {
      _leftNode.Execute();
      _rightNode.Execute();

      if (_leftNode.ValueType == ValueType.Boolean || _rightNode.ValueType == ValueType.Boolean)
      {
        throw new RuntimeException("An arithmetic operation cannot be performed on a boolean value");
      }

      ValueType = _leftNode.ValueType switch
      {
        ValueType.Integer => _rightNode.ValueType,
        ValueType.Float => ValueType.Float,
        _ => ValueType
      };

      switch (ValueType)
      {
        case ValueType.Integer:
          IntegerValue = CalculateIntegerValue();
          FloatValue = IntegerValue;
          break;
        case ValueType.Float:
          FloatValue = CalculateFloatValue();
          IntegerValue = (int) FloatValue;
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(ValueType), ValueType, "Unsupported value type");
      }

      BooleanValue = IntegerValue != 0;
    }
  }
}