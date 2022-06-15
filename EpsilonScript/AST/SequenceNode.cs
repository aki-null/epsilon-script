using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  public class SequenceNode : Node
  {
    private Node _leftNode;
    private Node _rightNode;

    public override bool IsConstant => _leftNode.IsConstant && _rightNode.IsConstant;

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IVariableContainer variables, IDictionary<uint, CustomFunctionOverload> functions)
    {
      if (!rpnStack.TryPop(out _rightNode) || !rpnStack.TryPop(out _leftNode))
      {
        throw new ParserException(element.Token, "Cannot find tokens to sequence");
      }
    }

    public override void Execute(IVariableContainer variablesOverride)
    {
      _leftNode.Execute(variablesOverride);
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

      _leftNode = _leftNode.Optimize();
      _rightNode = _rightNode.Optimize();
      return this;
    }
  }
}