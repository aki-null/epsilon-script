using System;
using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Helper;
using EpsilonScript.Parser;

namespace EpsilonScript.AST
{
  public class BooleanOperationNode : Node
  {
    private const string OperationTypeErrorMessage = "Boolean operation can only be performed on boolean values";
    private const string NodesNotAvailableErrorMessage = "Cannot find values to perform a boolean operation on";

    private Node _leftNode;
    private Node _rightNode;
    private ElementType _operationType;

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IDictionary<string, VariableValue> variables,
      IDictionary<string, CustomFunctionOverload> functions)
    {
      ValueType = ValueType.Boolean;
      _operationType = element.Type;

      if (!rpnStack.TryPop(out _rightNode) || !rpnStack.TryPop(out _leftNode))
      {
        throw new ParserException(element.Token, NodesNotAvailableErrorMessage);
      }
    }

    public override void Execute()
    {
      switch (_operationType)
      {
        case ElementType.BooleanOrOperator:
          _leftNode.Execute();
          if (_leftNode.ValueType != ValueType.Boolean)
          {
            throw new RuntimeException(OperationTypeErrorMessage);
          }

          if (_leftNode.BooleanValue)
          {
            BooleanValue = true;
            break;
          }

          _rightNode.Execute();
          if (_rightNode.ValueType != ValueType.Boolean)
          {
            throw new RuntimeException(OperationTypeErrorMessage);
          }

          BooleanValue = _rightNode.BooleanValue;
          break;
        case ElementType.BooleanAndOperator:
          _leftNode.Execute();
          if (_leftNode.ValueType != ValueType.Boolean)
          {
            throw new RuntimeException(OperationTypeErrorMessage);
          }

          if (!_leftNode.BooleanValue)
          {
            BooleanValue = false;
            break;
          }

          _rightNode.Execute();
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
  }
}