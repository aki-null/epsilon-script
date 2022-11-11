using System.Collections.Generic;
using EpsilonScript.Bytecode;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  internal class IntegerNode : Node
  {
    private int integerValue;

    private void Initialize(int value)
    {
      integerValue = value;
    }

    public IntegerNode()
    {
    }

    public IntegerNode(int value)
    {
      Initialize(value);
    }

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      CustomFunctionContainer functions)
    {
      Initialize(int.Parse(element.Token.Text.Span));
    }

    public override void Encode(MutableProgram program, ref byte nextRegisterIdx,
      VirtualMachine.VirtualMachine constantVm)
    {
      PushValue(program, ref nextRegisterIdx, integerValue);
    }
  }
}