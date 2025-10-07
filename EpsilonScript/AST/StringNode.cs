using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
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

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IVariableContainer variables, IDictionary<VariableId, CustomFunctionOverload> functions,
      Compiler.IntegerPrecision intPrecision, Compiler.FloatPrecision floatPrecision)
    {
      var span = element.Token.Text;
      // Slicing accounts for quotation marks
      StringValue = span.Slice(1, span.Length - 2).ToString();
    }
  }
}