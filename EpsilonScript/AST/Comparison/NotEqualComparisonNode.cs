using System;
using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST.Comparison
{
  /// <summary>
  /// Not equal comparison node.
  /// Compares two values for inequality. Supports numeric, boolean, and string types.
  /// Float/Double use fuzzy equality (IsNearlyEqual) to handle precision errors.
  /// </summary>
  internal sealed class NotEqualComparisonNode : ComparisonOperationNode
  {
    internal override OpCode LoweredOp => OpCode.NotEqual;
  }
}