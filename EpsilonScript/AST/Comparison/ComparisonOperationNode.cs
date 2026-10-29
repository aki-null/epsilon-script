using System.Collections.Generic;
using EpsilonScript.Intermediate;
using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST.Comparison
{
  /// <summary>
  /// Abstract base class for all comparison operation nodes.
  /// Provides common infrastructure for comparison operations.
  /// Concrete subclasses implement operation-specific comparison logic.
  /// </summary>
  internal abstract class ComparisonOperationNode : Node
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

    protected override void BuildCore(Stack<Node> rpnStack, Element element, CompilerContext context,
      Compiler.Options options, IVariableContainer variables)
    {
      ValueType = ExtendedType.Boolean;

      if (!rpnStack.TryPop(out RightNode) || !rpnStack.TryPop(out LeftNode))
      {
        throw new ParserException(element.Token, "Cannot find values to perform comparison operation on");
      }
    }

    public override Node Optimize()
    {
      if (IsPrecomputable)
      {
        Execute(null);
        return CreateValueNode();
      }

      LeftNode = LeftNode.Optimize();
      RightNode = RightNode.Optimize();
      return this;
    }

    public override void Validate()
    {
      LeftNode?.Validate();
      RightNode?.Validate();
    }
  }
}