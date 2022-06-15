using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  public class NegateNode : Node
  {
    private Node _childNode;

    public override bool IsConstant => _childNode.IsConstant;

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IVariableContainer variables, IDictionary<uint, CustomFunctionOverload> functions)
    {
      ValueType = ValueType.Boolean;

      if (!rpnStack.TryPop(out _childNode))
      {
        throw new ParserException(element.Token, "Cannot find value to perform negate operation on");
      }
    }

    public override void Execute(IVariableContainer variablesOverride)
    {
      _childNode.Execute(variablesOverride);
      if (_childNode.ValueType != ValueType.Boolean)
      {
        throw new RuntimeException("Cannot negate a non-boolean value");
      }

      BooleanValue = !_childNode.BooleanValue;
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

      _childNode = _childNode.Optimize();
      return this;
    }
  }
}