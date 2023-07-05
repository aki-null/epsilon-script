using System;
using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Bytecode;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.Tests
{
  internal class FakeStringNode : Node
  {
    private readonly string _stringValue;

    public FakeStringNode(string value)
    {
      _stringValue = value;
    }

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      CustomFunctionContainer functions)
    {
      throw new NotImplementedException("Fake nodes cannot be built from RPN stack");
    }

    public override void Encode(MutableProgram program, ref byte nextRegisterIdx,
      VirtualMachine.VirtualMachine constantVm)
    {
      var stringIndex = program.StringTable.Count;
      program.StringTable.Add(_stringValue);

      program.Instructions.Add(new Instruction
      {
        Type = InstructionType.LoadString,
        IntegerValue = stringIndex,
        reg0 = nextRegisterIdx
      });

      ++nextRegisterIdx;
    }
  }
}