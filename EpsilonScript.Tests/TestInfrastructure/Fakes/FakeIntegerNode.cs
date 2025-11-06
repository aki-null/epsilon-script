using System;
using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Intermediate;

namespace EpsilonScript.Tests.TestInfrastructure.Fakes
{
  internal class FakeIntegerNode : Node
  {
    public FakeIntegerNode(int value)
    {
      IntegerValue = value;
    }

    protected override void BuildCore(Stack<Node> rpnStack, Element element, CompilerContext context,
      Compiler.Options options, IVariableContainer variables)
    {
      throw new NotImplementedException("Fake nodes cannot be built from RPN srack");
    }
  }
}