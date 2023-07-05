using System;
using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Bytecode;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;
using ValueType = EpsilonScript.AST.ValueType;

namespace EpsilonScript.Tests
{
  internal class FakeFloatNode : Node
  {
    private readonly float _floatValue;

    public FakeFloatNode(float value)
    {
      _floatValue = value;
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
        Type = InstructionType.LoadFloat,
        FloatValue = _floatValue,
        reg0 = nextRegisterIdx
      });
      ++nextRegisterIdx;
    }
  }
}