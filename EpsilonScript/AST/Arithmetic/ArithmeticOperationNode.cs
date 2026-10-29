using System.Collections.Generic;
using EpsilonScript.Intermediate;
using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST.Arithmetic
{
  /// <summary>
  /// Abstract base class for all arithmetic operation nodes.
  /// Provides common infrastructure for binary arithmetic operations (Add, Subtract, Multiply, Divide, Modulo).
  /// Concrete subclasses choose their opcode and optimizations; the register VM implements the arithmetic.
  /// </summary>
  internal abstract class ArithmeticOperationNode : Node
  {
    protected Node LeftNode;
    protected Node RightNode;

    public override bool IsPrecomputable => LeftNode.IsPrecomputable && RightNode.IsPrecomputable;

    internal abstract OpCode LoweredOp { get; }

    internal override int Lower(RegisterCompiler compiler) =>
      compiler.LowerOperation(this, LoweredOp, LeftNode, RightNode);

    internal override void Reset()
    {
      base.Reset();
      LeftNode = null;
      RightNode = null;
    }

    /// <summary>
    /// Creates the multiply-add form of this node with the given addend and multiply operand.
    /// </summary>
    protected Node RentFused(NodeKind kind, Node addend, MultiplyNode multiplyNode) =>
      ((MultiplyAddOperationNode)RentNode(kind)).Initialize(addend, multiplyNode, this);

    protected override void BuildCore(Stack<Node> rpnStack, Element element, CompilerContext context,
      Compiler.Options options, IVariableContainer variables)
    {
      if (!rpnStack.TryPop(out RightNode) || !rpnStack.TryPop(out LeftNode))
      {
        throw new ParserException(element.Token, "Cannot find values to perform arithmetic operation on");
      }
    }

    public override void Validate()
    {
      LeftNode?.Validate();
      RightNode?.Validate();
    }
  }
}