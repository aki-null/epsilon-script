namespace EpsilonScript.AST.Arithmetic
{
  internal sealed class MultiplyNode : ArithmeticOperationNode
  {
    protected override string GetOperatorName() => "multiplication";

    protected override void CalculateInteger()
    {
      IntegerValue = _leftNode.IntegerValue * _rightNode.IntegerValue;
    }

    protected override void CalculateLong()
    {
      LongValue = _leftNode.LongValue * _rightNode.LongValue;
    }

    protected override void CalculateFloat()
    {
      FloatValue = _leftNode.FloatValue * _rightNode.FloatValue;
    }

    protected override void CalculateDouble()
    {
      DoubleValue = _leftNode.DoubleValue * _rightNode.DoubleValue;
    }

    protected override void CalculateDecimal()
    {
      DecimalValue = _leftNode.DecimalValue * _rightNode.DecimalValue;
    }

    public override Node Optimize()
    {
      if (IsPrecomputable)
      {
        Execute(null);
        return CreateValueNode();
      }

      _leftNode = _leftNode.Optimize();
      _rightNode = _rightNode.Optimize();
      return this;
    }
  }
}