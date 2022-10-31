using System;
using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;
using ValueType = EpsilonScript.AST.ValueType;

namespace EpsilonScript.Tests
{
  public class FakeStringNode : Node
  {
    public FakeStringNode(string value)
    {
      ValueType = ValueType.String;
      IntegerValue = 0;
      FloatValue = 0;
      BooleanValue = false;
      StringValue = value;
    }

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IVariableContainer variables, IDictionary<uint, CustomFunctionOverload> functions)
    {
      throw new NotImplementedException("Fake nodes cannot be built from RPN srack");
    }
  }
}