using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST.Arithmetic
{
  internal sealed class AddNode : ArithmeticOperationNode
  {
    internal override OpCode LoweredOp => OpCode.Add;

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

      // Try Multiply-Add optimization for addition operations
      // Pattern: (a * b) + c
      if (LeftNode is MultiplyNode left)
      {
        return RentFused(NodeKind.MultiplyAdd, RightNode, left).Optimize();
      }

      // Pattern: c + (a * b)
      if (RightNode is MultiplyNode right)
      {
        return RentFused(NodeKind.AddMultiply, LeftNode, right).Optimize();
      }

      return this;
    }
  }
}