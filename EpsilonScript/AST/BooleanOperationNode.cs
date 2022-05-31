using System;
using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  public class BooleanOperationNode : Node
  {
    private const string OperationTypeErrorMessage = "Boolean operation can only be performed on boolean values";
    private const string NodesNotAvailableErrorMessage = "Cannot find values to perform a boolean operation on";

    private Node _leftNode;
    private Node _rightNode;
    private ElementType _operationType;

    public override bool IsConstant => _leftNode.IsConstant && _rightNode.IsConstant;

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IDictionary<uint, VariableValue> variables,
      IDictionary<uint, CustomFunctionOverload> functions)
    {
      ValueType = ValueType.Boolean;
      _operationType = element.Type;

      if (!rpnStack.TryPop(out _rightNode) || !rpnStack.TryPop(out _leftNode))
      {
        throw new ParserException(element.Token, NodesNotAvailableErrorMessage);
      }
    }

    public override void Execute(IDictionary<uint, VariableValue> variablesOverride)
    {
      switch (_operationType)
      {
        case ElementType.BooleanOrOperator:
          _leftNode.Execute(variablesOverride);
          if (_leftNode.ValueType != ValueType.Boolean)
          {
            throw new RuntimeException(OperationTypeErrorMessage);
          }

          if (_leftNode.BooleanValue)
          {
            BooleanValue = true;
            break;
          }

          _rightNode.Execute(variablesOverride);
          if (_rightNode.ValueType != ValueType.Boolean)
          {
            throw new RuntimeException(OperationTypeErrorMessage);
          }

          BooleanValue = _rightNode.BooleanValue;
          break;
        case ElementType.BooleanAndOperator:
          _leftNode.Execute(variablesOverride);
          if (_leftNode.ValueType != ValueType.Boolean)
          {
            throw new RuntimeException(OperationTypeErrorMessage);
          }

          if (!_leftNode.BooleanValue)
          {
            BooleanValue = false;
            break;
          }

          _rightNode.Execute(variablesOverride);
          if (_rightNode.ValueType != ValueType.Boolean)
          {
            throw new RuntimeException(OperationTypeErrorMessage);
          }

          BooleanValue = _rightNode.BooleanValue;
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(_operationType), _operationType,
            "Unsupported boolean operation type");
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