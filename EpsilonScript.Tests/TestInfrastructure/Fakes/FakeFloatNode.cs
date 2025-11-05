using System;
using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.Tests.TestInfrastructure.Fakes
{
  internal class FakeFloatNode : Node
  {
    public FakeFloatNode(float value)
    {
      FloatValue = value;
    }

    protected override void BuildCore(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IVariableContainer variables, IDictionary<VariableId, CustomFunctionOverload> functions,
      Compiler.IntegerPrecision intPrecision, Compiler.FloatPrecision floatPrecision)
    {
      throw new NotImplementedException("Fake nodes cannot be built from RPN srack");
    }
  }
}