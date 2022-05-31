using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

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
      IDictionary<uint, VariableValue> variables,
      IDictionary<uint, CustomFunctionOverload> functions)
    {
      Initialize(int.Parse(element.Token.Text.Span));
    }
  }
}