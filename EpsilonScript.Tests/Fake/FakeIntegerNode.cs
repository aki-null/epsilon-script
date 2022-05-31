using System;
using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;
using ValueType = EpsilonScript.AST.ValueType;

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
      IDictionary<uint, VariableValue> variables,
      IDictionary<uint, CustomFunctionOverload> functions)
    {
      throw new NotImplementedException("Fake nodes cannot be built from RPN srack");
    }
  }
}