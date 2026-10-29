using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST.Arithmetic
{
  internal sealed class MultiplyNode : ArithmeticOperationNode
  {
    internal override OpCode LoweredOp => OpCode.Multiply;

    internal Node LeftChildNode => LeftNode;
    internal Node RightChildNode => RightNode;

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
  }
}