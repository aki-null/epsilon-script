using System.Collections.Generic;
using EpsilonScript.Parser;

namespace EpsilonScript.AST
{
  public class BooleanNode : Node
  {
    public override void Build(Stack<Node> rpnStack, Element element, IDictionary<string, VariableValue> variables,
      IDictionary<string, CustomFunction> functions)
    {
      ValueType = ValueType.Boolean;
      BooleanValue = element.Type == ElementType.BooleanLiteralTrue;
      IntegerValue = BooleanValue ? 1 : 0;
      FloatValue = IntegerValue;
    }
  }
}