using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Parser;

namespace EpsilonScript.AST
{
  public class IntegerNode : Node
  {
    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IDictionary<string, VariableValue> variables,
      IDictionary<string, CustomFunctionOverload> functions)
    {
      ValueType = ValueType.Integer;
      IntegerValue = int.Parse(element.Token.Text);
      FloatValue = IntegerValue;
      BooleanValue = IntegerValue != 0;
    }
  }
}