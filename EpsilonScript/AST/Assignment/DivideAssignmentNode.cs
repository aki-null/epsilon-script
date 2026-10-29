using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST.Assignment
{
  /// <summary>
  /// Divide assignment node: variable /= value
  /// Divides a numeric variable by a value.
  /// </summary>
  internal sealed class DivideAssignmentNode : AssignmentOperationNode
  {
    internal override OpCode LoweredOp => OpCode.AssignDivide;
  }
}