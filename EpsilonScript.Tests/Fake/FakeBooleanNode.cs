using System;
using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Bytecode;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.Tests
{
  internal class FakeBooleanNode : Node
  {
    private readonly bool _boolValue;

    public FakeBooleanNode(bool value)
    {
      _boolValue = value;
    }

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      CustomFunctionContainer functions)
    {
      throw new NotImplementedException("Fake nodes cannot be built from RPN stack");
    }

    public override void Encode(MutableProgram program, ref byte nextRegisterIdx,
      VirtualMachine.VirtualMachine constantVm)
    {
      program.Instructions.Add(new Instruction
      {
        Type = InstructionType.LoadBoolean,
        BooleanValue = _boolValue
      });
    }
  }
}