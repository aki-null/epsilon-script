using System.Collections.Generic;
using EpsilonScript.Intermediate;
using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST.Literal
{
  internal class NullNode : Node
  {
    internal override int Lower(RegisterCompiler compiler) => compiler.LowerConstant(this);

    protected override void BuildCore(Stack<Node> rpnStack, Element element, CompilerContext context,
      Compiler.Options options, IVariableContainer variables)
    {
      ValueType = ExtendedType.Null;
    }
  }
}