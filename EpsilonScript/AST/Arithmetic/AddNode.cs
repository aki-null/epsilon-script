using System;

namespace EpsilonScript.AST.Arithmetic
{
  internal sealed class AddNode : ArithmeticOperationNode
  {
    protected override string GetOperatorName() => "addition";

    protected override void CalculateInteger()
    {
      IntegerValue = _leftNode.IntegerValue + _rightNode.IntegerValue;
    }

    protected override void CalculateLong()
    {
      LongValue = _leftNode.LongValue + _rightNode.LongValue;
    }

    protected override void CalculateFloat()
    {
      FloatValue = _leftNode.FloatValue + _rightNode.FloatValue;
    }

    protected override void CalculateDouble()
    {
      DoubleValue = _leftNode.DoubleValue + _rightNode.DoubleValue;
    }

    protected override void CalculateDecimal()
    {
      DecimalValue = _leftNode.DecimalValue + _rightNode.DecimalValue;
    }

    protected override void CalculateString()
    {
      var sb = Context.StringBuilder;
      sb.Clear();

      // The left node is guaranteed to be a string node
      sb.Append(_leftNode.StringValue);
      switch (_rightNode.ValueType)
      {
        case ExtendedType.Integer:
          sb.Append(_rightNode.IntegerValue);
          break;
        case ExtendedType.Long:
          sb.Append(_rightNode.LongValue);
          break;
        case ExtendedType.Float:
          sb.AppendFloatInvariant(_rightNode.FloatValue, Context);
          break;
        case ExtendedType.Double:
          sb.AppendDoubleInvariant(_rightNode.DoubleValue, Context);
          break;
        case ExtendedType.Decimal:
          sb.AppendDecimalInvariant(_rightNode.DecimalValue, Context);
          break;
        case ExtendedType.String:
          sb.Append(_rightNode.StringValue);
          break;
        case ExtendedType.Boolean:
          sb.Append(_rightNode.BooleanValue ? "true" : "false");
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(_rightNode.ValueType), _rightNode.ValueType,
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
      _leftNode = _leftNode.Optimize();
      _rightNode = _rightNode.Optimize();

      // Try Multiply-Add optimization for addition operations
      // Pattern: (a * b) + c
      if (_leftNode is MultiplyNode left)
      {
        return new MultiplyAddNode(left.LeftNode, left.RightNode, _rightNode, left, this, Context).Optimize();
      }

      // Pattern: c + (a * b)
      if (_rightNode is MultiplyNode right)
      {
        return new AddMultiplyNode(right.LeftNode, right.RightNode, _leftNode, right, this, Context).Optimize();
      }

      return this;
    }
  }
}