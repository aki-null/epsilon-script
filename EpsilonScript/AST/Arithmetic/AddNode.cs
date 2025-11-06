using System;

namespace EpsilonScript.AST.Arithmetic
{
  internal sealed class AddNode : ArithmeticOperationNode
  {
    protected override string GetOperatorName() => "addition";

    protected override void CalculateInteger()
    {
      IntegerValue = LeftNode.IntegerValue + RightNode.IntegerValue;
    }

    protected override void CalculateLong()
    {
      LongValue = LeftNode.LongValue + RightNode.LongValue;
    }

    protected override void CalculateFloat()
    {
      FloatValue = LeftNode.FloatValue + RightNode.FloatValue;
    }

    protected override void CalculateDouble()
    {
      DoubleValue = LeftNode.DoubleValue + RightNode.DoubleValue;
    }

    protected override void CalculateDecimal()
    {
      DecimalValue = LeftNode.DecimalValue + RightNode.DecimalValue;
    }

    protected override void CalculateString()
    {
      var sb = Context.StringBuilder;
      sb.Clear();

      // The left node is guaranteed to be a string node
      sb.Append(LeftNode.StringValue);
      switch (RightNode.ValueType)
      {
        case ExtendedType.Integer:
          sb.Append(RightNode.IntegerValue);
          break;
        case ExtendedType.Long:
          sb.Append(RightNode.LongValue);
          break;
        case ExtendedType.Float:
          sb.AppendFloatInvariant(RightNode.FloatValue, Context);
          break;
        case ExtendedType.Double:
          sb.AppendDoubleInvariant(RightNode.DoubleValue, Context);
          break;
        case ExtendedType.Decimal:
          sb.AppendDecimalInvariant(RightNode.DecimalValue, Context);
          break;
        case ExtendedType.String:
          sb.Append(RightNode.StringValue);
          break;
        case ExtendedType.Boolean:
          sb.Append(RightNode.BooleanValue ? "true" : "false");
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(RightNode.ValueType), RightNode.ValueType,
            "Unsupported value type for string concatenation");
      }

      StringValue = sb.ToString();
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

      // Try Multiply-Add optimization for addition operations
      // Pattern: (a * b) + c
      if (LeftNode is MultiplyNode left)
      {
        return new MultiplyAddNode(RightNode, left, this, Context).Optimize();
      }

      // Pattern: c + (a * b)
      if (RightNode is MultiplyNode right)
      {
        return new AddMultiplyNode(LeftNode, right, this, Context).Optimize();
      }

      return this;
    }
  }
}