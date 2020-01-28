using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Parser;

namespace EpsilonScript.AST
{
  public class FloatNode : Node
  {
    private void Initialize(float value)
    {
      ValueType = ValueType.Float;
      FloatValue = value;
      IntegerValue = (int) FloatValue;
      BooleanValue = IntegerValue != 0;
    }

    public FloatNode()
    {
    }

    public FloatNode(float value)
    {
      Initialize(value);
    }

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IDictionary<string, VariableValue> variables,
      IDictionary<string, CustomFunctionOverload> functions)
    {
      Initialize(float.Parse(element.Token.Text));
    }
  }
}
