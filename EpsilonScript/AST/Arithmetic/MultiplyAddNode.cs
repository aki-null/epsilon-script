namespace EpsilonScript.AST.Arithmetic
{
  /// <summary>
  /// Multiply-add node: (a * b) + c
  /// Computes the product of two values and adds a third in a single operation.
  /// </summary>
  internal sealed class MultiplyAddNode : MultiplyAddOperationNode
  {
    public MultiplyAddNode(Node addend, MultiplyNode multiplyNode, ArithmeticOperationNode addSubtractNode,
      CompilerContext context) : base(addend, multiplyNode, addSubtractNode, context)
    {
    }

    protected override void CalculateInteger()
    {
      IntegerValue = Multiplier1.IntegerValue * Multiplier2.IntegerValue + Addend.IntegerValue;
    }

    protected override void CalculateLong()
    {
      LongValue = Multiplier1.LongValue * Multiplier2.LongValue + Addend.LongValue;
    }

    protected override void CalculateFloat()
    {
      FloatValue = Multiplier1.FloatValue * Multiplier2.FloatValue + Addend.FloatValue;
    }

    protected override void CalculateDouble()
    {
      DoubleValue = Multiplier1.DoubleValue * Multiplier2.DoubleValue + Addend.DoubleValue;
    }

    protected override void CalculateDecimal()
    {
      DecimalValue = Multiplier1.DecimalValue * Multiplier2.DecimalValue + Addend.DecimalValue;
    }
  }
}