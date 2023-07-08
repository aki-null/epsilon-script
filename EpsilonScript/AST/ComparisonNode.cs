using System;
using System.Collections.Generic;
using EpsilonScript.Bytecode;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  internal class ComparisonNode : Node
  {
    private Node _leftNode;
    private Node _rightNode;
    private ElementType _comparisonType;

    public override bool IsConstant => _leftNode.IsConstant && _rightNode.IsConstant;

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      CustomFunctionContainer functions)
    {
      _comparisonType = element.Type;

      if (!rpnStack.TryPop(out _rightNode) || !rpnStack.TryPop(out _leftNode))
      {
        throw new ParserException(element.Token, "Cannot find values to perform comparison operation on");
      }
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
      switch (_comparisonType)
      {
        case ElementType.ComparisonEqual:
          instructionType = InstructionType.ComparisonEqual;
          break;
        case ElementType.ComparisonNotEqual:
          instructionType = InstructionType.ComparisonNotEqual;
          break;
        case ElementType.ComparisonLessThan:
          instructionType = InstructionType.ComparisonLessThan;
          break;
        case ElementType.ComparisonGreaterThan:
          instructionType = InstructionType.ComparisonGreaterThan;
          break;
        case ElementType.ComparisonLessThanOrEqualTo:
          instructionType = InstructionType.ComparisonLessThanOrEqualTo;
          break;
        case ElementType.ComparisonGreaterThanOrEqualTo:
          instructionType = InstructionType.ComparisonGreaterThanOrEqualTo;
          break;
        default:
          throw new ArgumentOutOfRangeException("Unsupported comparison operator", nameof(_comparisonType));
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

      // Any comparison instruction consumes two registers and stores the result into a single register. This results in
      // one less register usage.
      --nextRegisterIdx;
    }
  }
}