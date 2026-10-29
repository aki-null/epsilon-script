using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST.Comparison
{
  /// <summary>
  /// Greater than or equal comparison node.
  /// Compares two numeric values. Only supports numeric types.
  /// </summary>
  internal sealed class GreaterThanOrEqualComparisonNode : ComparisonOperationNode
  {
    internal override OpCode LoweredOp => OpCode.GreaterThanOrEqual;
  }
}