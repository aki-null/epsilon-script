using System.Collections.Generic;
using EpsilonScript.Bytecode;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  internal class SequenceNode : Node
  {
    private Node _leftNode;
    private Node _rightNode;

    public override bool IsConstant => _leftNode.IsConstant && _rightNode.IsConstant;

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      CustomFunctionContainer functions)
    {
      if (!rpnStack.TryPop(out _rightNode) || !rpnStack.TryPop(out _leftNode))
      {
        throw new ParserException(element.Token, "Cannot find tokens to sequence");
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
      // Discard the execution result from above
      --nextRegisterIdx;
      _rightNode.Encode(program, ref nextRegisterIdx, constantVm);
    }
  }
}