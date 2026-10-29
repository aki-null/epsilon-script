using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST.Assignment
{
  /// <summary>
  /// Direct assignment node: variable = value
  /// Assigns a value to a variable, supporting all variable types.
  /// </summary>
  internal sealed class DirectAssignmentNode : AssignmentOperationNode
  {
    internal override OpCode LoweredOp => OpCode.AssignDirect;
  }
}