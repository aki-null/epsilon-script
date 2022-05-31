using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  public class BooleanNode : Node
  {
    private void Initialize(bool value)
    {
      ValueType = ValueType.Boolean;
      BooleanValue = value;
      IntegerValue = BooleanValue ? 1 : 0;
      FloatValue = IntegerValue;
    }

    public BooleanNode()
    {
    }

    public BooleanNode(bool value)
    {
      Initialize(value);
    }

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IDictionary<uint, VariableValue> variables,
      IDictionary<uint, CustomFunctionOverload> functions)
    {
      Initialize(element.Type == ElementType.BooleanLiteralTrue);
    }
  }
}