using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  public class SequenceNode : Node
  {
    private Node _leftNode;
    private Node _rightNode;
    private bool _isSingleNode;

    public override bool IsConstant =>
      _isSingleNode ? _rightNode.IsConstant : (_leftNode.IsConstant && _rightNode.IsConstant);

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IVariableContainer variables, IDictionary<VariableId, CustomFunctionOverload> functions)
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
      ValueType = _rightNode.ValueType;

      IntegerValue = _rightNode.IntegerValue;
      FloatValue = _rightNode.FloatValue;
      BooleanValue = _rightNode.BooleanValue;
      TupleValue = _rightNode.TupleValue;
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