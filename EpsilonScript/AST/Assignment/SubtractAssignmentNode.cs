using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST.Assignment
{
  /// <summary>
  /// Subtract assignment node: variable -= value
  /// Subtracts a value from a numeric variable.
  /// </summary>
  internal sealed class SubtractAssignmentNode : AssignmentOperationNode
  {
    internal override OpCode LoweredOp => OpCode.AssignSubtract;
  }
}