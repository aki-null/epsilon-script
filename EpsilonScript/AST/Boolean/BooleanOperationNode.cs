using System.Collections.Generic;
using EpsilonScript.AST.Literal;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST.Boolean
{
  /// <summary>
  /// Abstract base class for binary boolean operations (&& and ||).
  /// Provides shared logic for boolean operations with short-circuit evaluation.
  /// </summary>
  internal abstract class BooleanOperationNode : Node
  {
    protected const string OperationTypeErrorMessage = "Boolean operation can only be performed on boolean values";
    private const string NodesNotAvailableErrorMessage = "Cannot find values to perform a boolean operation on";

    protected Node LeftNode;
    protected Node RightNode;

    public override bool IsPrecomputable => LeftNode.IsPrecomputable && RightNode.IsPrecomputable;

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
        throw new ParserException(element.Token, NodesNotAvailableErrorMessage);
      }
    }

    /// <summary>
    /// Helper to ensure constant boolean node is executed. A literal already holds its value; executing one would
    /// lower and run it on the VM.
    /// </summary>
    protected void EnsureExecuted(Node node)
    {
      if (node.IsPrecomputable && node.ValueType == ExtendedType.Boolean && !(node is BooleanNode))
      {
        node.ExecuteInPlace();
      }
    }

    public override void Validate()
    {
      LeftNode?.Validate();
      RightNode?.Validate();
    }
  }
}