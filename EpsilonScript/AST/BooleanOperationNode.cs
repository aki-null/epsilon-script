using System.Collections.Generic;
using EpsilonScript.Bytecode;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  internal class BooleanOperationNode : Node
  {
    private const string NodesNotAvailableErrorMessage = "Cannot find values to perform a boolean operation on";

    private Node _leftNode;
    private Node _rightNode;
    private ElementType _operationType;

    public override bool IsConstant => _leftNode.IsConstant && _rightNode.IsConstant;

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      CustomFunctionContainer functions)
    {
      _operationType = element.Type;

      if (!rpnStack.TryPop(out _rightNode) || !rpnStack.TryPop(out _leftNode))
      {
        throw new ParserException(element.Token, NodesNotAvailableErrorMessage);
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

      // Temporary instruction: short circuit jump
      var shortCircuitJumpInstructionIndex = program.Instructions.Count;
      program.Instructions.Add(new Instruction());

      // The last register used, which holds the execution result of left node can now be discarded, because right
      // node execution is done if short circuit has failed, and boolean operation result is now fully dependent on the
      // right node execution result.
      --nextRegisterIdx;

      _rightNode.Encode(program, ref nextRegisterIdx, constantVm);

      switch (_operationType)
      {
        case ElementType.BooleanOrOperator:
          // Inject a jump to a location after the left node program, evaluating whether short circuit is possible.
          // If the left node evaluates to TRUE, the right node evaluation can be skipped.
          // The jump target location is the instruction after this.
          program.Instructions[shortCircuitJumpInstructionIndex] = new Instruction
          {
            Type = InstructionType.JumpIf,
            IntegerValue = program.Instructions.Count
          };
          break;
        case ElementType.BooleanAndOperator:
          // Inject a jump to a location after the left node program, evaluating whether short circuit is possible.
          // If the left node evaluates to FALSE, the right node evaluation can be skipped.
          // The jump target location is the instruction after this.
          program.Instructions[shortCircuitJumpInstructionIndex] = new Instruction
          {
            Type = InstructionType.JumpNotIf,
            IntegerValue = program.Instructions.Count
          };
          break;
      }
    }
  }
}