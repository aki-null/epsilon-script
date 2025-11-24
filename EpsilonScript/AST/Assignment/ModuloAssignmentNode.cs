using System;

namespace EpsilonScript.AST.Assignment
{
  /// <summary>
  /// Modulo assignment node: variable %= value
  /// Computes the remainder of dividing a numeric variable by a value.
  /// </summary>
  internal sealed class ModuloAssignmentNode : AssignmentOperationNode
  {
    protected override bool RequiresNumericOperand() => true;

    protected override void PerformAssignment(VariableValue variable)
    {
      switch (variable.Type)
      {
        case Type.Integer:
        {
          var rightValue = RightNode.IntegerValue;
          if (rightValue == 0)
            throw new DivideByZeroException("Modulo by zero");
          variable.IntegerValue %= rightValue;
          break;
        }
        case Type.Long:
        {
          var rightValue = RightNode.LongValue;
          if (rightValue == 0)
            throw new DivideByZeroException("Modulo by zero");
          variable.LongValue %= rightValue;
          break;
        }
        case Type.Float:
        {
          var rightValue = RightNode.FloatValue;
          if (rightValue == 0.0f)
            throw new DivideByZeroException("Modulo by zero");
          variable.FloatValue %= rightValue;
          break;
        }
        case Type.Double:
        {
          var rightValue = RightNode.DoubleValue;
          if (rightValue == 0.0)
            throw new DivideByZeroException("Modulo by zero");
          variable.DoubleValue %= rightValue;
          break;
        }
        case Type.Decimal:
        {
          var rightValue = RightNode.DecimalValue;
          if (rightValue == 0m)
            throw new DivideByZeroException("Modulo by zero");
          variable.DecimalValue %= rightValue;
          break;
        }
        default:
          throw new ArgumentOutOfRangeException(nameof(variable.Type), variable.Type, "Unsupported variable type");
      }
    }
  }
}