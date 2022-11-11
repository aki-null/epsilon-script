using System.Collections.Generic;
using EpsilonScript.Bytecode;
using EpsilonScript.Function;
using EpsilonScript.Helper;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  internal class VariableNode : Node
  {
    public int VariableName { get; private set; }

    public override bool IsConstant => false;

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      CustomFunctionContainer functions)
    {
      VariableName = element.Token.Text.ToString().GetUniqueIdentifier();
    }

    public override void Encode(MutableProgram program, ref byte nextRegisterIdx,
      VirtualMachine.VirtualMachine constantVm)
    {
      program.Instructions.Add(new Instruction
      {
        Type = InstructionType.LoadVariableValue,
        IntegerValue = VariableName,
        reg0 = nextRegisterIdx
      });
      ++nextRegisterIdx;
    }
  }
}