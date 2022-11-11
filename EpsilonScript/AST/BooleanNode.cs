using System.Collections.Generic;
using EpsilonScript.Bytecode;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  internal class BooleanNode : Node
  {
    private bool _booleanValue;

    private void Initialize(bool value)
    {
      _booleanValue = value;
    }

    public BooleanNode()
    {
    }

    public BooleanNode(bool value)
    {
      Initialize(value);
    }

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      CustomFunctionContainer functions)
    {
      Initialize(element.Type == ElementType.BooleanLiteralTrue);
    }

    public override void Encode(MutableProgram program, ref byte nextRegisterIdx,
      VirtualMachine.VirtualMachine constantVm)
    {
      PushValue(program, ref nextRegisterIdx, _booleanValue);
    }
  }
}