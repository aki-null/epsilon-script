using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST.Comparison
{
  /// <summary>
  /// Less than or equal comparison node.
  /// Compares two numeric values. Only supports numeric types.
  /// </summary>
  internal sealed class LessThanOrEqualComparisonNode : ComparisonOperationNode
  {
    internal override OpCode LoweredOp => OpCode.LessThanOrEqual;
  }
}