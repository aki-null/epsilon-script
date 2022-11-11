using System;
using System.Collections.Generic;
using EpsilonScript.Bytecode;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  internal class ArithmeticNode : Node
  {
    private Node _leftNode;
    private Node _rightNode;
    private ElementType _operator;

    public override bool IsConstant => _leftNode.IsConstant && _rightNode.IsConstant;

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      CustomFunctionContainer functions)
    {
      if (!rpnStack.TryPop(out _rightNode) || !rpnStack.TryPop(out _leftNode))
      {
        throw new ParserException(element.Token, "Cannot find values to perform arithmetic operation on");
      }

      _operator = element.Type;
    }

    public override void Encode(MutableProgram program, ref byte nextRegisterIdx,
      VirtualMachine.VirtualMachine constantVm)
    {
      if (TryEncodeConstant(program, ref nextRegisterIdx, constantVm))
      {
        return;
      }

      _leftNode.Encode(program, ref nextRegisterIdx, constantVm);
      _rightNode.Encode(program, ref nextRegisterIdx, constantVm);

      switch (_operator)
      {
        case ElementType.AddOperator:
          program.Instructions.Add(new Instruction
          {
            Type = InstructionType.Add,
            reg0 = (byte)(nextRegisterIdx - 2),
            reg1 = (byte)(nextRegisterIdx - 2),
            reg2 = (byte)(nextRegisterIdx - 1)
          });
          break;
        case ElementType.SubtractOperator:
          program.Instructions.Add(new Instruction
          {
            Type = InstructionType.Subtract,
            reg0 = (byte)(nextRegisterIdx - 2),
            reg1 = (byte)(nextRegisterIdx - 2),
            reg2 = (byte)(nextRegisterIdx - 1)
          });
          break;
        case ElementType.MultiplyOperator:
          program.Instructions.Add(new Instruction
          {
            Type = InstructionType.Multiply,
            reg0 = (byte)(nextRegisterIdx - 2),
            reg1 = (byte)(nextRegisterIdx - 2),
            reg2 = (byte)(nextRegisterIdx - 1)
          });
          break;
        case ElementType.DivideOperator:
          program.Instructions.Add(new Instruction
          {
            Type = InstructionType.Divide,
            reg0 = (byte)(nextRegisterIdx - 2),
            reg1 = (byte)(nextRegisterIdx - 2),
            reg2 = (byte)(nextRegisterIdx - 1)
          });
          break;
        case ElementType.ModuloOperator:
          program.Instructions.Add(new Instruction
          {
            Type = InstructionType.Modulo,
            reg0 = (byte)(nextRegisterIdx - 2),
            reg1 = (byte)(nextRegisterIdx - 2),
            reg2 = (byte)(nextRegisterIdx - 1)
          });
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }

      --nextRegisterIdx;
    }
  }
}