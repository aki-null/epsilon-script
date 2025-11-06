using System;

namespace EpsilonScript.AST.Arithmetic
{
  /// <summary>
  /// Add-multiply node: c + (a * b)
  /// Computes the product of two values and adds to a left operand in a single operation.
  /// Supports string concatenation when c is a string.
  /// </summary>
  internal sealed class AddMultiplyNode : MultiplyAddOperationNode
  {
    public AddMultiplyNode(Node multiplier1, Node multiplier2, Node addend,
      MultiplyNode multiplyNode, ArithmeticOperationNode addSubtractNode, CompilerContext context)
      : base(multiplier1, multiplier2, addend, multiplyNode, addSubtractNode, context)
    {
    }

    protected override bool SupportsStrings() => true;

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

    protected override void CalculateString(ExtendedType multiplyType)
    {
      // NoAlloc validation: Block runtime string concatenation
      if (NoAllocMode)
      {
        throw new RuntimeException(
          "String concatenation is not allowed in NoAlloc mode (causes runtime heap allocation)", Location);
      }

      // String concatenation: addend is string, multiply result is numeric
      // Pattern: string + (numeric * numeric)
      // This maintains compatibility with ArithmeticNode: only "string + X" is allowed

      var sb = Context.StringBuilder;
      sb.Clear();
      sb.Append(Addend.StringValue);

      switch (multiplyType)
      {
        case ExtendedType.Integer:
          sb.Append(Multiplier1.IntegerValue * Multiplier2.IntegerValue);
          break;
        case ExtendedType.Long:
          sb.Append(Multiplier1.LongValue * Multiplier2.LongValue);
          break;
        case ExtendedType.Float:
          sb.AppendFloatInvariant(Multiplier1.FloatValue * Multiplier2.FloatValue, Context);
          break;
        case ExtendedType.Double:
          sb.AppendDoubleInvariant(Multiplier1.DoubleValue * Multiplier2.DoubleValue, Context);
          break;
        case ExtendedType.Decimal:
          sb.AppendDecimalInvariant(Multiplier1.DecimalValue * Multiplier2.DecimalValue, Context);
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(multiplyType), multiplyType,
            "Unsupported value type for string concatenation");
      }

      StringValue = sb.ToString();
    }
  }
}