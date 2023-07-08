using System;
using System.Collections.Generic;
using EpsilonScript.Bytecode;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  internal class AssignmentNode : Node
  {
    private VariableNode _assignmentTarget;
    private Node _rightNode;
    private ElementType _assignmentType;

    public override bool IsConstant => false;

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      CustomFunctionContainer functions)
    {
      if ((options & Compiler.Options.Immutable) == Compiler.Options.Immutable)
      {
        throw new ParserException(element.Token, "An assignment operator cannot be used for an immutable script");
      }

      _assignmentType = element.Type;

      if (!rpnStack.TryPop(out _rightNode) || !rpnStack.TryPop(out var leftNode))
      {
        throw new ParserException(element.Token, "Cannot find elements to perform assignment operation on");
      }

      if (leftNode is VariableNode variableNode)
      {
        _assignmentTarget = variableNode;
      }
      else
      {
        throw new ParserException(element.Token, "A value can only be assigned to a variable");
      }
    }

    public override void Encode(MutableProgram program, ref byte nextRegisterIdx,
      VirtualMachine.VirtualMachine constantVm)
    {
      if (TryEncodeConstant(program, ref nextRegisterIdx, constantVm))
      {
        return;
      }

      _rightNode.Encode(program, ref nextRegisterIdx, constantVm);

      if (_assignmentType == ElementType.AssignmentOperator)
      {
        program.Instructions.Add(new Instruction
        {
          Type = InstructionType.AssignVariable,
          IntegerValue = _assignmentTarget.VariableName,
          reg0 = (byte) (nextRegisterIdx - 1)
        });
      }
      else
      {
        // Compound assignment requires the assignment target variable to be loaded into the stack
        program.Instructions.Add(new Instruction
        {
          Type = InstructionType.LoadVariableValue,
          IntegerValue = _assignmentTarget.VariableName,
          reg0 = nextRegisterIdx
        });
        ++nextRegisterIdx;

        InstructionType instructionType;
        // Arithmetic instructions for non-simple assignment
        switch (_assignmentType)
        {
          case ElementType.AssignmentAddOperator:
            instructionType = InstructionType.Add;
            break;
          case ElementType.AssignmentSubtractOperator:
            instructionType = InstructionType.Subtract;
            break;
          case ElementType.AssignmentMultiplyOperator:
            instructionType = InstructionType.Multiply;
            break;
          case ElementType.AssignmentDivideOperator:
            instructionType = InstructionType.Divide;
            break;
          default:
            throw new ArgumentOutOfRangeException("Unsupported assignment operator", nameof(_assignmentType));
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

        program.Instructions.Add(new Instruction
        {
          Type = InstructionType.AssignVariable,
          IntegerValue = _assignmentTarget.VariableName,
          reg0 = (writeReg)
        });
      }
      // Note: Assignment instruction does not require the next register index to be decremented, since the result of
      // assignment IS the value assigned.
    }
  }
}