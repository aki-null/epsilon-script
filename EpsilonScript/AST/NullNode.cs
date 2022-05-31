using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  public class NullNode : Node
  {
    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IDictionary<uint, VariableValue> variables,
      IDictionary<uint, CustomFunctionOverload> functions)
    {
      ValueType = ValueType.Null;
    }
  }
}