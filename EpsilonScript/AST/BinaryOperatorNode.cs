using System;
using System.Collections.Generic;
using EpsilonScript.Bytecode;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  internal class BinaryOperatorNode : Node
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
        throw new ParserException(element.Token, "Cannot find values to perform binary operation on");
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

      InstructionType instructionType = _operator switch
      {
        ElementType.ComparisonEqual => InstructionType.ComparisonEqual,
        ElementType.ComparisonNotEqual => InstructionType.ComparisonNotEqual,
        ElementType.ComparisonLessThan => InstructionType.ComparisonLessThan,
        ElementType.ComparisonGreaterThan => InstructionType.ComparisonGreaterThan,
        ElementType.ComparisonLessThanOrEqualTo => InstructionType.ComparisonLessThanOrEqualTo,
        ElementType.ComparisonGreaterThanOrEqualTo => InstructionType.ComparisonGreaterThanOrEqualTo,
        ElementType.AddOperator => InstructionType.Add,
        ElementType.SubtractOperator => InstructionType.Subtract,
        ElementType.MultiplyOperator => InstructionType.Multiply,
        ElementType.DivideOperator => InstructionType.Divide,
        ElementType.ModuloOperator => InstructionType.Modulo,
        _ => throw new ArgumentOutOfRangeException("Unsupported binary operator", nameof(_operator))
      };

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

      // Any binary operation consumes two registers and stores the result into a single register. This results in one
      // less register usage.
      --nextRegisterIdx;
    }
  }
}