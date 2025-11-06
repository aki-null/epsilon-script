namespace EpsilonScript.AST.Arithmetic
{
  internal sealed class SubtractNode : ArithmeticOperationNode
  {
    protected override string GetOperatorName() => "subtraction";

    protected override void CalculateInteger()
    {
      IntegerValue = _leftNode.IntegerValue - _rightNode.IntegerValue;
    }

    protected override void CalculateLong()
    {
      LongValue = _leftNode.LongValue - _rightNode.LongValue;
    }

    protected override void CalculateFloat()
    {
      FloatValue = _leftNode.FloatValue - _rightNode.FloatValue;
    }

    protected override void CalculateDouble()
    {
      DoubleValue = _leftNode.DoubleValue - _rightNode.DoubleValue;
    }

    protected override void CalculateDecimal()
    {
      DecimalValue = _leftNode.DecimalValue - _rightNode.DecimalValue;
    }

    public override Node Optimize()
    {
      if (IsPrecomputable)
      {
        Execute(null);
        return CreateValueNode();
      }

      // Optimize children first
      _leftNode = _leftNode.Optimize();
      _rightNode = _rightNode.Optimize();

      // Try Multiply-Subtract optimization for subtraction operations
      // Pattern: (a * b) - c
      if (_leftNode is MultiplyNode left)
      {
        return new MultiplySubtractNode(left.LeftNode, left.RightNode, _rightNode, left, this, Context).Optimize();
      }

      // Pattern: c - (a * b)
      if (_rightNode is MultiplyNode right)
      {
        return new SubtractMultiplyNode(right.LeftNode, right.RightNode, _leftNode, right, this, Context).Optimize();
      }

      return this;
    }
  }
}