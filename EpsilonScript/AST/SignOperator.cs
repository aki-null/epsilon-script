using System.Collections.Generic;
using EpsilonScript.Bytecode;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  internal class SignOperator : Node
  {
    private Node _childNode;
    private ElementType _operationType;

    public override bool IsConstant => _childNode.IsConstant;

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      CustomFunctionContainer functions)
    {
      if (!rpnStack.TryPop(out _childNode))
      {
        throw new ParserException(element.Token, "Cannot find value to perform sign operation on");
      }

      _operationType = element.Type;
    }

    public override void Encode(MutableProgram program, ref byte nextRegisterIdx,
      VirtualMachine.VirtualMachine constantVm)
    {
      if (TryEncodeConstant(program, ref nextRegisterIdx, constantVm))
      {
        return;
      }

      _childNode.Encode(program, ref nextRegisterIdx, constantVm);

      if (_operationType == ElementType.NegativeOperator)
      {
        program.Instructions.Add(new Instruction
        {
          Type = InstructionType.Negative,
          reg0 = (byte)(nextRegisterIdx - 1),
          reg1 = (byte)(nextRegisterIdx - 1)
        });
      }
      // Positive operator does not do anything, so no instruction is generated
    }
  }
}