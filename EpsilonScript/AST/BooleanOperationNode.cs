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
      IVariableContainer variables, IDictionary<VariableId, CustomFunctionOverload> functions)
    {
      ValueType = ValueType.Boolean;
      _operationType = element.Type;

      if (!rpnStack.TryPop(out _rightNode) || !rpnStack.TryPop(out _leftNode))
      {
        throw new ParserException(element.Token, NodesNotAvailableErrorMessage);
      }
    }

    public override void Execute(IVariableContainer variablesOverride)
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
      // Helper to ensure constant boolean node is executed
      void EnsureExecuted(Node node)
      {
        if (node.IsConstant && node.ValueType == ValueType.Boolean)
        {
          node.Execute(null);
        }
      }

      // Check for early short-circuit opportunities before optimizing children
      // This prevents unnecessary work when we can already determine the result
      EnsureExecuted(_rightNode);
      EnsureExecuted(_leftNode);

      switch (_operationType)
      {
        case ElementType.BooleanAndOperator:
          // false && anything => false (don't need to optimize right side)
          if (_leftNode.IsConstant && _leftNode.ValueType == ValueType.Boolean && !_leftNode.BooleanValue)
          {
            return new BooleanNode(false);
          }
          // anything && false => false (don't need to optimize left side)
          if (_rightNode.IsConstant && _rightNode.ValueType == ValueType.Boolean && !_rightNode.BooleanValue)
          {
            return new BooleanNode(false);
          }
          break;

        case ElementType.BooleanOrOperator:
          // true || anything => true (don't need to optimize right side)
          if (_leftNode.IsConstant && _leftNode.ValueType == ValueType.Boolean && _leftNode.BooleanValue)
          {
            return new BooleanNode(true);
          }
          // anything || true => true (don't need to optimize left side)
          if (_rightNode.IsConstant && _rightNode.ValueType == ValueType.Boolean && _rightNode.BooleanValue)
          {
            return new BooleanNode(true);
          }
          break;
      }

      // Optimize child nodes
      _leftNode = _leftNode.Optimize();
      _rightNode = _rightNode.Optimize();

      // Handle type errors for constant expressions
      if (IsConstant && (_leftNode.ValueType != ValueType.Boolean || _rightNode.ValueType != ValueType.Boolean))
      {
        Execute(null); // This will throw RuntimeException for type mismatches
        return CreateValueNode();
      }

      // Ensure optimized constant nodes are executed
      EnsureExecuted(_leftNode);
      EnsureExecuted(_rightNode);

      // Short-circuit optimizations after child optimization
      switch (_operationType)
      {
        case ElementType.BooleanAndOperator:
          // true && expression => expression
          if (_leftNode.IsConstant && _leftNode.ValueType == ValueType.Boolean && _leftNode.BooleanValue)
          {
            return _rightNode;
          }
          // expression && true => expression
          if (_rightNode.IsConstant && _rightNode.ValueType == ValueType.Boolean && _rightNode.BooleanValue)
          {
            return _leftNode;
          }
          break;

        case ElementType.BooleanOrOperator:
          // false || expression => expression
          if (_leftNode.IsConstant && _leftNode.ValueType == ValueType.Boolean && !_leftNode.BooleanValue)
          {
            return _rightNode;
          }
          // expression || false => expression
          if (_rightNode.IsConstant && _rightNode.ValueType == ValueType.Boolean && !_rightNode.BooleanValue)
          {
            return _leftNode;
          }
          break;
      }

      // Constant folding: if both operands are constant boolean, evaluate at compile time
      if (IsConstant && _leftNode.ValueType == ValueType.Boolean && _rightNode.ValueType == ValueType.Boolean)
      {
        Execute(null);
        return CreateValueNode();
      }

      return this;
    }
  }
}