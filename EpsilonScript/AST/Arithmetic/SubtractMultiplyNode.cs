using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST.Arithmetic
{
  /// <summary>
  /// Subtract-multiply node: c - (a * b)
  /// Subtracts the product of two values from a left operand in a single operation.
  /// </summary>
  internal sealed class SubtractMultiplyNode : MultiplyAddOperationNode
  {
    internal override OpCode LoweredOp => OpCode.SubtractMultiply;
  }
}