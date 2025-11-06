using System;

namespace EpsilonScript.AST.Comparison
{
  /// <summary>
  /// Not equal comparison node.
  /// Compares two values for inequality. Supports numeric, boolean, and string types.
  /// Float/Double use fuzzy equality (IsNearlyEqual) to handle precision errors.
  /// </summary>
  internal sealed class NotEqualComparisonNode : ComparisonOperationNode
  {
    protected override bool SupportsNonNumericTypes() => true;

    protected override bool CompareInteger() => LeftNode.IntegerValue != RightNode.IntegerValue;
    protected override bool CompareLong() => LeftNode.LongValue != RightNode.LongValue;
    protected override bool CompareFloat() => !Math.IsNearlyEqual(LeftNode.FloatValue, RightNode.FloatValue);
    protected override bool CompareDouble() => !Math.IsNearlyEqual(LeftNode.DoubleValue, RightNode.DoubleValue);
    protected override bool CompareDecimal() => LeftNode.DecimalValue != RightNode.DecimalValue;
    protected override bool CompareBoolean() => LeftNode.BooleanValue != RightNode.BooleanValue;

    protected override bool CompareString() =>
      !string.Equals(LeftNode.StringValue, RightNode.StringValue, StringComparison.Ordinal);
  }
}