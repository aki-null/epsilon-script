using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST.Assignment
{
  /// <summary>
  /// Modulo assignment node: variable %= value
  /// Computes the remainder of dividing a numeric variable by a value.
  /// </summary>
  internal sealed class ModuloAssignmentNode : AssignmentOperationNode
  {
    internal override OpCode LoweredOp => OpCode.AssignModulo;
  }
}