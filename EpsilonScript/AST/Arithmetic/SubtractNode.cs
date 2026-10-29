using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST.Arithmetic
{
  internal sealed class SubtractNode : ArithmeticOperationNode
  {
    internal override OpCode LoweredOp => OpCode.Subtract;

    public override Node Optimize()
    {
      if (IsPrecomputable)
      {
        Execute(null);
        return CreateValueNode();
      }

      // Optimize children first
      LeftNode = LeftNode.Optimize();
      RightNode = RightNode.Optimize();

      // Try Multiply-Subtract optimization for subtraction operations
      // Pattern: (a * b) - c
      if (LeftNode is MultiplyNode left)
      {
        return RentFused(NodeKind.MultiplySubtract, RightNode, left).Optimize();
      }

      // Pattern: c - (a * b)
      if (RightNode is MultiplyNode right)
      {
        return RentFused(NodeKind.SubtractMultiply, LeftNode, right).Optimize();
      }

      return this;
    }
  }
}