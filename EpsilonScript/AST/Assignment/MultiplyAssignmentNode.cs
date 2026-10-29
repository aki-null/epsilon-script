using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST.Assignment
{
  /// <summary>
  /// Multiply assignment node: variable *= value
  /// Multiplies a numeric variable by a value.
  /// </summary>
  internal sealed class MultiplyAssignmentNode : AssignmentOperationNode
  {
    internal override OpCode LoweredOp => OpCode.AssignMultiply;
  }
}