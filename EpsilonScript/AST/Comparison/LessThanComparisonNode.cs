using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST.Comparison
{
  /// <summary>
  /// Less than comparison node.
  /// Compares two numeric values. Only supports numeric types.
  /// </summary>
  internal sealed class LessThanComparisonNode : ComparisonOperationNode
  {
    internal override OpCode LoweredOp => OpCode.LessThan;
  }
}