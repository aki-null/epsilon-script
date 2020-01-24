using System.Collections.Generic;
using EpsilonScript.Parser;

namespace EpsilonScript.AST
{
  public class NullNode : Node
  {
    public override void Build(Stack<Node> rpnStack, Element element, IDictionary<string, VariableValue> variables,
      IDictionary<string, CustomFunction> functions)
    {
      ValueType = ValueType.Null;
    }
  }
}