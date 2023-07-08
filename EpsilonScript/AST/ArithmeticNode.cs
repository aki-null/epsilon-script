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

      InstructionType instructionType;
      switch (_operator)
      {
        case ElementType.AddOperator:
          instructionType = InstructionType.Add;
          break;
        case ElementType.SubtractOperator:
          instructionType = InstructionType.Subtract;
          break;
        case ElementType.MultiplyOperator:
          instructionType = InstructionType.Multiply;
          break;
        case ElementType.DivideOperator:
          instructionType = InstructionType.Divide;
          break;
        case ElementType.ModuloOperator:
          instructionType = InstructionType.Modulo;
          break;
        default:
          throw new ArgumentOutOfRangeException("Unsupported arithmetic operator", nameof(_operator));
      }

      var leftReg = (byte)(nextRegisterIdx - 2);
      var rightReg = (byte)(nextRegisterIdx - 1);
      var writeReg = (byte)(nextRegisterIdx - 2);

      program.Instructions.Add(new Instruction
      {
        Type = instructionType,
        reg0 = writeReg,
        reg1 = leftReg,
        reg2 = rightReg
      });

      // Any arithmetic instruction consumes two registers and stores the result into a single register. This results
      // in one less register usage.
      --nextRegisterIdx;
    }
  }
}