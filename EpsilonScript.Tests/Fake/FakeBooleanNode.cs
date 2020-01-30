using EpsilonScript.Parser;
using EpsilonScript.AST;
using EpsilonScript.Function;
using System.Collections.Generic;

namespace EpsilonScript.Tests
{
  public class FakeBooleanNode : Node
  {
    public FakeBooleanNode(bool value)
    {
      ValueType = ValueType.Boolean;
      BooleanValue = value;
      IntegerValue = BooleanValue ? 1 : 0;
      FloatValue = IntegerValue;
    }

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IDictionary<string, VariableValue> variables,
      IDictionary<string, CustomFunctionOverload> functions)
    {
      throw new System.NotImplementedException("Fake nodes cannot be built from RPN srack");
    }
  }
}
