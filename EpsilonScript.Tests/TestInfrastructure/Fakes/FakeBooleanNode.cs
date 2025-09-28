using System;
using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;
using ValueType = EpsilonScript.AST.ValueType;

namespace EpsilonScript.Tests.TestInfrastructure.Fakes
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
      IVariableContainer variables, IDictionary<VariableId, CustomFunctionOverload> functions)
    {
      throw new NotImplementedException("Fake nodes cannot be built from RPN stack");
    }
  }
}