using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  public class StringNode : Node
  {
    private void Initialize(string value)
    {
      ValueType = ValueType.String;
      StringValue = value;
    }

    public StringNode()
    {
    }

    public StringNode(string value)
    {
      Initialize(value);
    }

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IVariableContainer variables, IDictionary<VariableId, CustomFunctionOverload> functions)
    {
      var span = element.Token.Text;
      // Slicing accounts for quotation marks
      Initialize(span.Slice(1, span.Length - 2).ToString());
    }
  }
}