using System.Collections.Generic;
using EpsilonScript.Bytecode;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  /// <summary>
  /// Tuple only appears inside a function.
  /// This is used to build a list of parameters for function invocation.
  /// </summary>
  internal class TupleNode : Node
  {
    private Node _leftNode;
    private Node _rightNode;

    public override bool IsConstant => _leftNode.IsConstant && _rightNode.IsConstant;

    public int Count
    {
      get
      {
        if (_leftNode is TupleNode leftTupleNode)
        {
          return leftTupleNode.Count + 1;
        }

        return 2;
      }
    }

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      CustomFunctionContainer functions)
    {
      if (!rpnStack.TryPop(out _rightNode) || !rpnStack.TryPop(out _leftNode))
      {
        throw new ParserException(element.Token, "Cannot find values to create parameter list");
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
    }
  }
}