using System.Collections.Generic;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST.Literal
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

    protected override void BuildCore(Stack<Node> rpnStack, Element element, CompilerContext context,
      Compiler.Options options, IVariableContainer variables)
    {
      BooleanValue = element.Type == ElementType.BooleanLiteralTrue;
    }
  }
}