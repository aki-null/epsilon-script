using System;
using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST.Comparison
{
  /// <summary>
  /// Equal comparison node.
  /// Compares two values for equality. Supports numeric, boolean, and string types.
  /// Float/Double use fuzzy equality (IsNearlyEqual) to handle precision errors.
  /// </summary>
  internal sealed class EqualComparisonNode : ComparisonOperationNode
  {
    internal override OpCode LoweredOp => OpCode.Equal;
  }
}