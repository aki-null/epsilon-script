using System.Collections.Generic;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST.Literal
{
  internal class StringNode : Node
  {
    public StringNode()
    {
    }

    public StringNode(string value)
    {
      StringValue = value;
    }

    protected override void BuildCore(Stack<Node> rpnStack, Element element, CompilerContext context,
      Compiler.Options options, IVariableContainer variables)
    {
      var span = element.Token.Text;
      // Slicing accounts for quotation marks
      StringValue = span.Slice(1, span.Length - 2).ToString();
    }
  }
}