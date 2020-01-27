using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Parser;

namespace EpsilonScript.AST
{
  public class FloatNode : Node
  {
    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IDictionary<string, VariableValue> variables,
      IDictionary<string, CustomFunctionOverload> functions)
    {
      ValueType = ValueType.Float;
      FloatValue = float.Parse(element.Token.Text);
      IntegerValue = (int) FloatValue;
      BooleanValue = IntegerValue != 0;
    }
  }
}