using System.Collections.Generic;
using EpsilonScript.Intermediate;
using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST.Assignment
{
  /// <summary>
  /// Abstract base class for all assignment operation nodes.
  /// Provides common infrastructure for assignment operations (=, +=, -=, *=, /=, %=).
  /// Concrete subclasses choose the assignment opcode; the register VM performs the assignment.
  /// </summary>
  internal abstract class AssignmentOperationNode : Node
  {
    private Node _leftNode;
    protected Node RightNode;

    public override bool IsPrecomputable => false;

    internal abstract OpCode LoweredOp { get; }

    internal override int Lower(RegisterCompiler compiler) =>
      _leftNode.LowerAssignmentTarget(compiler, this, RightNode);

    internal override void Reset()
    {
      base.Reset();
      _leftNode = null;
      RightNode = null;
    }

    protected override void BuildCore(Stack<Node> rpnStack, Element element, CompilerContext context,
      Compiler.Options options, IVariableContainer variables)
    {
      if ((options & Compiler.Options.Immutable) == Compiler.Options.Immutable)
      {
        throw new ParserException(element.Token, "An assignment operator cannot be used for an immutable script");
      }

      if (!rpnStack.TryPop(out RightNode) || !rpnStack.TryPop(out _leftNode))
      {
        throw new ParserException(element.Token, "Cannot find elements to perform assignment operation on");
      }
    }

    public override void Validate()
    {
      _leftNode?.Validate();
      RightNode?.Validate();
    }
  }
}