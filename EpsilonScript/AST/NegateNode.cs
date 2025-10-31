using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  internal class NegateNode : Node
  {
    private Node _childNode;

    public override bool IsPrecomputable => _childNode.IsPrecomputable;

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IVariableContainer variables, IDictionary<VariableId, CustomFunctionOverload> functions,
      Compiler.IntegerPrecision intPrecision, Compiler.FloatPrecision floatPrecision)
    {
      ValueType = ExtendedType.Boolean;

      if (!rpnStack.TryPop(out _childNode))
      {
        throw new ParserException(element.Token, "Cannot find value to perform negate operation on");
      }
    }

    public override void Execute(IVariableContainer variablesOverride)
    {
      _childNode.Execute(variablesOverride);
      if (_childNode.ValueType != ExtendedType.Boolean)
      {
        throw new RuntimeException("Cannot negate a non-boolean value");
      }

      BooleanValue = !_childNode.BooleanValue;
    }

    public override Node Optimize()
    {
      if (IsPrecomputable)
      {
        Execute(null);
        return CreateValueNode();
      }

      _childNode = _childNode.Optimize();
      return this;
    }
  }
}