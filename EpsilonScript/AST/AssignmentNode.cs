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

      // Anything that is not a simple assignment requires the variable value to be read into the stack
      if (_assignmentType != ElementType.AssignmentOperator)
      {
        program.Instructions.Add(new Instruction
        {
          Type = InstructionType.LoadVariableValue,
          IntegerValue = _assignmentTarget.VariableName,
          reg0 = nextRegisterIdx
        });
        ++nextRegisterIdx;
      }

      // Arithmetic instructions for non-simple assignment
      switch (_assignmentType)
      {
        case ElementType.AssignmentAddOperator:
          program.Instructions.Add(new Instruction
          {
            Type = InstructionType.Add,
            reg0 = (byte)(nextRegisterIdx - 2),
            reg1 = (byte)(nextRegisterIdx - 1),
            reg2 = (byte)(nextRegisterIdx - 2)
          });
          --nextRegisterIdx;
          break;
        case ElementType.AssignmentSubtractOperator:
          program.Instructions.Add(new Instruction
          {
            Type = InstructionType.Subtract,
            reg0 = (byte)(nextRegisterIdx - 2),
            reg1 = (byte)(nextRegisterIdx - 1),
            reg2 = (byte)(nextRegisterIdx - 2)
          });
          --nextRegisterIdx;
          break;
        case ElementType.AssignmentMultiplyOperator:
          program.Instructions.Add(new Instruction
          {
            Type = InstructionType.Multiply,
            reg0 = (byte)(nextRegisterIdx - 2),
            reg1 = (byte)(nextRegisterIdx - 1),
            reg2 = (byte)(nextRegisterIdx - 2)
          });
          --nextRegisterIdx;
          break;
        case ElementType.AssignmentDivideOperator:
          program.Instructions.Add(new Instruction
          {
            Type = InstructionType.Divide,
            reg0 = (byte)(nextRegisterIdx - 2),
            reg1 = (byte)(nextRegisterIdx - 1),
            reg2 = (byte)(nextRegisterIdx - 2)
          });
          --nextRegisterIdx;
          break;
      }

      program.Instructions.Add(new Instruction
      {
        Type = InstructionType.AssignVariable,
        IntegerValue = _assignmentTarget.VariableName,
        reg0 = (byte)(nextRegisterIdx - 1)
      });
    }
  }
}