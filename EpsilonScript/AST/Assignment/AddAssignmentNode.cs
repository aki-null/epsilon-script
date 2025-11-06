using System;

namespace EpsilonScript.AST.Assignment
{
  /// <summary>
  /// Add assignment node: variable += value
  /// Adds a value to a numeric variable.
  /// </summary>
  internal sealed class AddAssignmentNode : AssignmentOperationNode
  {
    protected override bool RequiresNumericOperand() => true;

    protected override void PerformAssignment(VariableValue variable)
    {
      switch (variable.Type)
      {
        case Type.Integer:
          variable.IntegerValue += RightNode.IntegerValue;
          break;
        case Type.Long:
          variable.LongValue += RightNode.LongValue;
          break;
        case Type.Float:
          variable.FloatValue += RightNode.FloatValue;
          break;
        case Type.Double:
          variable.DoubleValue += RightNode.DoubleValue;
          break;
        case Type.Decimal:
          variable.DecimalValue += RightNode.DecimalValue;
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(variable.Type), variable.Type, "Unsupported variable type");
      }
    }
  }
}