using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST.Arithmetic
{
  /// <summary>
  /// Multiply-subtract node: (a * b) - c
  /// Computes the product of two values and subtracts a third in a single operation.
  /// </summary>
  internal sealed class MultiplySubtractNode : MultiplyAddOperationNode
  {
    internal override OpCode LoweredOp => OpCode.MultiplySubtract;
  }
}