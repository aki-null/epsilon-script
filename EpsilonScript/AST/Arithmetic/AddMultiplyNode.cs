using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST.Arithmetic
{
  /// <summary>
  /// Add-multiply node: c + (a * b)
  /// Computes the product of two values and adds to a left operand in a single operation.
  /// Supports string concatenation when c is a string.
  /// </summary>
  internal sealed class AddMultiplyNode : MultiplyAddOperationNode
  {
    internal override OpCode LoweredOp => OpCode.AddMultiply;
  }
}