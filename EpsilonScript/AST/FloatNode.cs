using System.Collections.Generic;
using EpsilonScript.Bytecode;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  internal class FloatNode : Node
  {
    private float _floatValue;

    private void Initialize(float value)
    {
      _floatValue = value;
    }

    public FloatNode()
    {
    }

    public FloatNode(float value)
    {
      Initialize(value);
    }

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      CustomFunctionContainer functions)
    {
      Initialize(float.Parse(element.Token.Text.Span));
    }

    public override void Encode(MutableProgram program, ref byte nextRegisterIdx,
      VirtualMachine.VirtualMachine constantVm)
    {
      PushValue(program, ref nextRegisterIdx, _floatValue);
    }
  }
}