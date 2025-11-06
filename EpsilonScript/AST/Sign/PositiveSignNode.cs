namespace EpsilonScript.AST.Sign
{
  /// <summary>
  /// Unary positive sign operator: +x
  /// Returns the value unchanged (no-op operation).
  /// </summary>
  internal sealed class PositiveSignNode : SignOperationNode
  {
    protected override void ApplySignInteger()
    {
      IntegerValue = ChildNode.IntegerValue;
    }

    protected override void ApplySignLong()
    {
      LongValue = ChildNode.LongValue;
    }

    protected override void ApplySignFloat()
    {
      FloatValue = ChildNode.FloatValue;
    }

    protected override void ApplySignDouble()
    {
      DoubleValue = ChildNode.DoubleValue;
    }

    protected override void ApplySignDecimal()
    {
      DecimalValue = ChildNode.DecimalValue;
    }

    public override Node Optimize()
    {
      if (IsPrecomputable)
      {
        Execute(null);
        return CreateValueNode();
      }

      // Unary positive operator is a no-op, just return the optimized child
      return ChildNode.Optimize();
    }
  }
}