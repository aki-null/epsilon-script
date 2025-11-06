namespace EpsilonScript.AST.Sign
{
  /// <summary>
  /// Unary negative sign operator: -x
  /// Negates the value (changes its sign).
  /// </summary>
  internal sealed class NegativeSignNode : SignOperationNode
  {
    protected override void ApplySignInteger()
    {
      IntegerValue = -ChildNode.IntegerValue;
    }

    protected override void ApplySignLong()
    {
      LongValue = -ChildNode.LongValue;
    }

    protected override void ApplySignFloat()
    {
      FloatValue = -ChildNode.FloatValue;
    }

    protected override void ApplySignDouble()
    {
      DoubleValue = -ChildNode.DoubleValue;
    }

    protected override void ApplySignDecimal()
    {
      DecimalValue = -ChildNode.DecimalValue;
    }

    public override Node Optimize()
    {
      if (IsPrecomputable)
      {
        Execute(null);
        return CreateValueNode();
      }

      ChildNode = ChildNode.Optimize();
      return this;
    }
  }
}