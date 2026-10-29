using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST.Sign
{
  /// <summary>
  /// Unary positive sign operator: +x
  /// Returns the value unchanged (no-op operation).
  /// </summary>
  internal sealed class PositiveSignNode : SignOperationNode
  {
    internal override OpCode LoweredOp => OpCode.Positive;

    public override Node Optimize()
    {
      if (IsPrecomputable)
      {
        Execute(null);
        return CreateValueNode();
      }

      // Unary positive operator is a no-op, just return the optimized child
      return ChildNode.Optimize();
    }
  }
}