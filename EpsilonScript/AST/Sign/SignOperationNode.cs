using System.Collections.Generic;
using EpsilonScript.Intermediate;
using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST.Sign
{
  /// <summary>
  /// Abstract base class for unary sign operations (+x, -x).
  /// Provides shared logic for applying sign operations to numeric values.
  /// </summary>
  internal abstract class SignOperationNode : Node
  {
    protected Node ChildNode;

    public override bool IsPrecomputable => ChildNode.IsPrecomputable;

    internal abstract OpCode LoweredOp { get; }

    internal override int Lower(RegisterCompiler compiler) => compiler.LowerOperation(this, LoweredOp, ChildNode);

    internal override void Reset()
    {
      base.Reset();
      ChildNode = null;
    }

    protected override void BuildCore(Stack<Node> rpnStack, Element element, CompilerContext context,
      Compiler.Options options, IVariableContainer variables)
    {
      if (!rpnStack.TryPop(out ChildNode))
      {
        throw new ParserException(element.Token, "Cannot find value to perform sign operation on");
      }
    }

    public override void Validate()
    {
      ChildNode?.Validate();
    }
  }
}