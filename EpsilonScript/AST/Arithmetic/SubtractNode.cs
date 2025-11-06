namespace EpsilonScript.AST.Arithmetic
{
  internal sealed class SubtractNode : ArithmeticOperationNode
  {
    protected override string GetOperatorName() => "subtraction";

    protected override void CalculateInteger()
    {
      IntegerValue = LeftNode.IntegerValue - RightNode.IntegerValue;
    }

    protected override void CalculateLong()
    {
      LongValue = LeftNode.LongValue - RightNode.LongValue;
    }

    protected override void CalculateFloat()
    {
      FloatValue = LeftNode.FloatValue - RightNode.FloatValue;
    }

    protected override void CalculateDouble()
    {
      DoubleValue = LeftNode.DoubleValue - RightNode.DoubleValue;
    }

    protected override void CalculateDecimal()
    {
      DecimalValue = LeftNode.DecimalValue - RightNode.DecimalValue;
    }

    public override Node Optimize()
    {
      if (IsPrecomputable)
      {
        Execute(null);
        return CreateValueNode();
      }

      // Optimize children first
      LeftNode = LeftNode.Optimize();
      RightNode = RightNode.Optimize();

      // Try Multiply-Subtract optimization for subtraction operations
      // Pattern: (a * b) - c
      if (LeftNode is MultiplyNode left)
      {
        return new MultiplySubtractNode(RightNode, left, this, Context).Optimize();
      }

      // Pattern: c - (a * b)
      if (RightNode is MultiplyNode right)
      {
        return new SubtractMultiplyNode(LeftNode, right, this, Context).Optimize();
      }

      return this;
    }
  }
}