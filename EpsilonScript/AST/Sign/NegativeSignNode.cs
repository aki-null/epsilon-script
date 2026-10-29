using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST.Sign
{
  /// <summary>
  /// Unary negative sign operator: -x
  /// Negates the value (changes its sign).
  /// </summary>
  internal sealed class NegativeSignNode : SignOperationNode
  {
    internal override OpCode LoweredOp => OpCode.Negate;

    public override Node Optimize()
    {
      if (IsPrecomputable)
      {
        Execute(null);
        return CreateValueNode();
      }

      ChildNode = ChildNode.Optimize();
      return this;
    }
  }
}