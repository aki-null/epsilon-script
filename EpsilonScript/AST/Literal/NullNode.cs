using System.Collections.Generic;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST.Literal
{
  internal class NullNode : Node
  {
    protected override void BuildCore(Stack<Node> rpnStack, Element element, CompilerContext context,
      Compiler.Options options, IVariableContainer variables)
    {
      ValueType = ExtendedType.Null;
    }
  }
}