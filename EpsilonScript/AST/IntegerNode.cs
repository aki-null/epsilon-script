using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Parser;

namespace EpsilonScript.AST
{
  public class IntegerNode : Node
  {
    private void Initialize(int value)
    {
      ValueType = ValueType.Integer;
      IntegerValue = value;
      FloatValue = IntegerValue;
      BooleanValue = IntegerValue != 0;
    }

    public IntegerNode()
    {
    }

    public IntegerNode(int value)
    {
      Initialize(value);
    }

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IDictionary<string, VariableValue> variables,
      IDictionary<string, CustomFunctionOverload> functions)
    {
      Initialize(int.Parse(element.Token.Text));
    }
  }
}
