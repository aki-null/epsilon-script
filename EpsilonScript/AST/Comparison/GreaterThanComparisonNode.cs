using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST.Comparison
{
  /// <summary>
  /// Greater than comparison node.
  /// Compares two numeric values. Only supports numeric types.
  /// </summary>
  internal sealed class GreaterThanComparisonNode : ComparisonOperationNode
  {
    internal override OpCode LoweredOp => OpCode.GreaterThan;
  }
}