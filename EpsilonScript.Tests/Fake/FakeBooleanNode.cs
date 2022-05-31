using System;
using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;
using ValueType = EpsilonScript.AST.ValueType;

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
      IDictionary<uint, VariableValue> variables,
      IDictionary<uint, CustomFunctionOverload> functions)
    {
      throw new NotImplementedException("Fake nodes cannot be built from RPN srack");
    }
  }
}