using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Helper;
using EpsilonScript.Parser;

namespace EpsilonScript.AST
{
  public class SequenceNode : Node
  {
    private Node _leftNode;
    private Node _rightNode;

    public override bool IsConstant => _leftNode.IsConstant && _rightNode.IsConstant;

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IDictionary<string, VariableValue> variables,
      IDictionary<string, CustomFunctionOverload> functions)
    {
      if (!rpnStack.TryPop(out _rightNode) || !rpnStack.TryPop(out _leftNode))
      {
        throw new ParserException(element.Token, "Cannot find tokens to sequence");
      }
    }

    public override void Execute()
    {
      _leftNode.Execute();
      _rightNode.Execute();
      ValueType = _rightNode.ValueType;

      IntegerValue = _rightNode.IntegerValue;
      FloatValue = _rightNode.FloatValue;
      BooleanValue = _rightNode.BooleanValue;
      TupleValue = _rightNode.TupleValue;
    }
  }
}