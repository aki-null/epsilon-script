namespace EpsilonScript.AST.Arithmetic
{
  internal sealed class MultiplyNode : ArithmeticOperationNode
  {
    protected override string GetOperatorName() => "multiplication";

    internal Node LeftChildNode => LeftNode;
    internal Node RightChildNode => RightNode;

    protected override void CalculateInteger()
    {
      IntegerValue = LeftNode.IntegerValue * RightNode.IntegerValue;
    }

    protected override void CalculateLong()
    {
      LongValue = LeftNode.LongValue * RightNode.LongValue;
    }

    protected override void CalculateFloat()
    {
      FloatValue = LeftNode.FloatValue * RightNode.FloatValue;
    }

    protected override void CalculateDouble()
    {
      DoubleValue = LeftNode.DoubleValue * RightNode.DoubleValue;
    }

    protected override void CalculateDecimal()
    {
      DecimalValue = LeftNode.DecimalValue * RightNode.DecimalValue;
    }

    public override Node Optimize()
    {
      if (IsPrecomputable)
      {
        Execute(null);
        return CreateValueNode();
      }

      LeftNode = LeftNode.Optimize();
      RightNode = RightNode.Optimize();
      return this;
    }
  }
}