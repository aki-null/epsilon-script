using System.Collections.Generic;
using EpsilonScript.Parser;

namespace EpsilonScript.AST
{
  public class IntegerNode : Node
  {
    public override void Build(Stack<Node> rpnStack, Element element, IDictionary<string, VariableValue> variables,
      IDictionary<string, CustomFunction> functions)
    {
      ValueType = ValueType.Integer;
      IntegerValue = int.Parse(element.Token.Text);
      FloatValue = IntegerValue;
      BooleanValue = IntegerValue != 0;
    }
  }
}