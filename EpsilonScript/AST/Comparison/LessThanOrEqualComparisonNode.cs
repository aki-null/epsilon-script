namespace EpsilonScript.AST.Comparison
{
  /// <summary>
  /// Less than or equal comparison node.
  /// Compares two numeric values. Only supports numeric types.
  /// </summary>
  internal sealed class LessThanOrEqualComparisonNode : ComparisonOperationNode
  {
    protected override bool CompareInteger() => LeftNode.IntegerValue <= RightNode.IntegerValue;
    protected override bool CompareLong() => LeftNode.LongValue <= RightNode.LongValue;
    protected override bool CompareFloat() => LeftNode.FloatValue <= RightNode.FloatValue;
    protected override bool CompareDouble() => LeftNode.DoubleValue <= RightNode.DoubleValue;
    protected override bool CompareDecimal() => LeftNode.DecimalValue <= RightNode.DecimalValue;
  }
}