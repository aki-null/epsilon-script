using System.Collections.Generic;
using EpsilonScript.Bytecode;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  internal class NegateNode : Node
  {
    private Node _childNode;

    public override bool IsConstant => _childNode.IsConstant;

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      CustomFunctionContainer functions)
    {
      if (!rpnStack.TryPop(out _childNode))
      {
        throw new ParserException(element.Token, "Cannot find value to perform negate operation on");
      }
    }

    public override void Encode(MutableProgram program, ref byte nextRegisterIdx,
      VirtualMachine.VirtualMachine constantVm)
    {
      if (TryEncodeConstant(program, ref nextRegisterIdx, constantVm))
      {
        return;
      }

      _childNode.Encode(program, ref nextRegisterIdx, constantVm);
      program.Instructions.Add(new Instruction
      {
        Type = InstructionType.Negate,
        reg0 = (byte)(nextRegisterIdx - 1),
        reg1 = (byte)(nextRegisterIdx - 1)
      });
    }
  }
}