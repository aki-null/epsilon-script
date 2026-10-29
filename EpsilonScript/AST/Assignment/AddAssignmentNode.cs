using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST.Assignment
{
  /// <summary>
  /// Add assignment node: variable += value
  /// Adds a value to a numeric variable.
  /// </summary>
  internal sealed class AddAssignmentNode : AssignmentOperationNode
  {
    internal override OpCode LoweredOp => OpCode.AssignAdd;
  }
}