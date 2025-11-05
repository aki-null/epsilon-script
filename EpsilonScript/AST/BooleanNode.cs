using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  internal class BooleanNode : Node
  {
    public BooleanNode()
    {
    }

    public BooleanNode(bool value)
    {
      BooleanValue = value;
    }

    protected override void BuildCore(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IVariableContainer variables, IDictionary<VariableId, CustomFunctionOverload> functions,
      Compiler.IntegerPrecision intPrecision, Compiler.FloatPrecision floatPrecision)
    {
      BooleanValue = element.Type == ElementType.BooleanLiteralTrue;
    }
  }
}