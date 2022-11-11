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

      switch (_comparisonType)
      {
        case ElementType.ComparisonEqual:
          program.Instructions.Add(new Instruction
          {
            Type = InstructionType.ComparisonEqual,
            reg0 = (byte)(nextRegisterIdx - 2),
            reg1 = (byte)(nextRegisterIdx - 2),
            reg2 = (byte)(nextRegisterIdx - 1)
          });
          break;
        case ElementType.ComparisonNotEqual:
          program.Instructions.Add(new Instruction
          {
            Type = InstructionType.ComparisonNotEqual,
            reg0 = (byte)(nextRegisterIdx - 2),
            reg1 = (byte)(nextRegisterIdx - 2),
            reg2 = (byte)(nextRegisterIdx - 1)
          });
          break;
        case ElementType.ComparisonLessThan:
          program.Instructions.Add(new Instruction
          {
            Type = InstructionType.ComparisonLessThan,
            reg0 = (byte)(nextRegisterIdx - 2),
            reg1 = (byte)(nextRegisterIdx - 2),
            reg2 = (byte)(nextRegisterIdx - 1)
          });
          break;
        case ElementType.ComparisonGreaterThan:
          program.Instructions.Add(new Instruction
          {
            Type = InstructionType.ComparisonGreaterThan,
            reg0 = (byte)(nextRegisterIdx - 2),
            reg1 = (byte)(nextRegisterIdx - 2),
            reg2 = (byte)(nextRegisterIdx - 1)
          });
          break;
        case ElementType.ComparisonLessThanOrEqualTo:
          program.Instructions.Add(new Instruction
          {
            Type = InstructionType.ComparisonLessThanOrEqualTo,
            reg0 = (byte)(nextRegisterIdx - 2),
            reg1 = (byte)(nextRegisterIdx - 2),
            reg2 = (byte)(nextRegisterIdx - 1)
          });
          break;
        case ElementType.ComparisonGreaterThanOrEqualTo:
          program.Instructions.Add(new Instruction
          {
            Type = InstructionType.ComparisonGreaterThanOrEqualTo,
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