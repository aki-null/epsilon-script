using System.Collections.Generic;
using EpsilonScript.Intermediate;
using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST.Literal
{
  internal class BooleanNode : Node
  {
    internal override int Lower(RegisterCompiler compiler) => compiler.LowerConstant(this);

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