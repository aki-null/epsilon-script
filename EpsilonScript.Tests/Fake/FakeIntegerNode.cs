using EpsilonScript.Parser;
using EpsilonScript.AST;
using EpsilonScript.Function;
using System.Collections.Generic;

namespace EpsilonScript.Tests
{
  public class FakeIntegerNode : Node
  {
    public FakeIntegerNode(int value)
    {
      ValueType = ValueType.Integer;
      IntegerValue = value;
      FloatValue = value;
      BooleanValue = IntegerValue != 0;
    }

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IDictionary<string, VariableValue> variables,
      IDictionary<string, CustomFunctionOverload> functions)
    {
      throw new System.NotImplementedException("Fake nodes cannot be built from RPN srack");
    }
  }
}
