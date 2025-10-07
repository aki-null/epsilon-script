using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  internal class SequenceNode : Node
  {
    private Node _leftNode;
    private Node _rightNode;
    private bool _isSingleNode;

    public override bool IsConstant =>
      _isSingleNode ? _rightNode.IsConstant : (_leftNode.IsConstant && _rightNode.IsConstant);

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IVariableContainer variables, IDictionary<VariableId, CustomFunctionOverload> functions,
      Compiler.IntegerPrecision intPrecision, Compiler.FloatPrecision floatPrecision)
    {
      if (!rpnStack.TryPop(out _rightNode))
      {
        throw new ParserException(element.Token, "Cannot find tokens to sequence");
      }

      // Handle trailing semicolon - if there's only one operand, treat it as a no-op
      if (!rpnStack.TryPop(out _leftNode))
      {
        _isSingleNode = true;
        _leftNode = null;
      }
    }

    public override void Execute(IVariableContainer variablesOverride)
    {
      if (!_isSingleNode)
      {
        _leftNode.Execute(variablesOverride);
      }

      _rightNode.Execute(variablesOverride);

      switch (_rightNode.ValueType)
      {
        case ExtendedType.Integer:
          IntegerValue = _rightNode.IntegerValue;
          break;
        case ExtendedType.Long:
          LongValue = _rightNode.LongValue;
          break;
        case ExtendedType.Float:
          FloatValue = _rightNode.FloatValue;
          break;
        case ExtendedType.Double:
          DoubleValue = _rightNode.DoubleValue;
          break;
        case ExtendedType.Decimal:
          DecimalValue = _rightNode.DecimalValue;
          break;
        case ExtendedType.Boolean:
          BooleanValue = _rightNode.BooleanValue;
          break;
        case ExtendedType.String:
          StringValue = _rightNode.StringValue;
          break;
        case ExtendedType.Tuple:
          TupleValue = _rightNode.TupleValue;
          ValueType = ExtendedType.Tuple;
          break;
        case ExtendedType.Null:
          ValueType = ExtendedType.Null;
          break;
        default:
          ValueType = _rightNode.ValueType;
          break;
      }
    }

    public override Node Optimize()
    {
      if (IsConstant)
      {
        Execute(null);
        return CreateValueNode();
      }

      if (_isSingleNode)
      {
        return _rightNode.Optimize();
      }

      _leftNode = _leftNode.Optimize();
      _rightNode = _rightNode.Optimize();
      return this;
    }
  }
}