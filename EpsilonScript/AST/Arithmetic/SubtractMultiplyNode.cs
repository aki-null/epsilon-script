namespace EpsilonScript.AST.Arithmetic
{
  /// <summary>
  /// Subtract-multiply node: c - (a * b)
  /// Subtracts the product of two values from a left operand in a single operation.
  /// </summary>
  internal sealed class SubtractMultiplyNode : MultiplyAddOperationNode
  {
    public SubtractMultiplyNode(Node multiplier1, Node multiplier2, Node addend, MultiplyNode multiplyNode,
      ArithmeticOperationNode addSubtractNode, CompilerContext context) : base(multiplier1, multiplier2, addend,
      multiplyNode, addSubtractNode, context)
    {
    }

    protected override string GetStringErrorMessage()
    {
      return "String operations only support concatenation (+), not subtraction";
    }

    protected override void CalculateInteger()
    {
      IntegerValue = Addend.IntegerValue - Multiplier1.IntegerValue * Multiplier2.IntegerValue;
    }

    protected override void CalculateLong()
    {
      LongValue = Addend.LongValue - Multiplier1.LongValue * Multiplier2.LongValue;
    }

    protected override void CalculateFloat()
    {
      FloatValue = Addend.FloatValue - Multiplier1.FloatValue * Multiplier2.FloatValue;
    }

    protected override void CalculateDouble()
    {
      DoubleValue = Addend.DoubleValue - Multiplier1.DoubleValue * Multiplier2.DoubleValue;
    }

    protected override void CalculateDecimal()
    {
      DecimalValue = Addend.DecimalValue - Multiplier1.DecimalValue * Multiplier2.DecimalValue;
    }
  }
}