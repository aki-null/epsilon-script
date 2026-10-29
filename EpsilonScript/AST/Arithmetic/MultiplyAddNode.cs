using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST.Arithmetic
{
  /// <summary>
  /// Multiply-add node: (a * b) + c
  /// Computes the product of two values and adds a third in a single operation.
  /// </summary>
  internal sealed class MultiplyAddNode : MultiplyAddOperationNode
  {
    internal override OpCode LoweredOp => OpCode.MultiplyAdd;
  }
}