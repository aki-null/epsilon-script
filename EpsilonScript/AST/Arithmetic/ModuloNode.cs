using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST.Arithmetic
{
  internal sealed class ModuloNode : ArithmeticOperationNode
  {
    internal override OpCode LoweredOp => OpCode.Modulo;

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