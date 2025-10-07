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

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IVariableContainer variables, IDictionary<VariableId, CustomFunctionOverload> functions,
      Compiler.IntegerPrecision intPrecision, Compiler.FloatPrecision floatPrecision)
    {
      BooleanValue = element.Type == ElementType.BooleanLiteralTrue;
    }
  }
}