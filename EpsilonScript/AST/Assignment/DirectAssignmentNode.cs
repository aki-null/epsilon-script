using System;

namespace EpsilonScript.AST.Assignment
{
  /// <summary>
  /// Direct assignment node: variable = value
  /// Assigns a value to a variable, supporting all variable types.
  /// </summary>
  internal sealed class DirectAssignmentNode : AssignmentOperationNode
  {
    protected override void PerformAssignment(VariableValue variable)
    {
      switch (variable.Type)
      {
        case Type.Integer:
          variable.IntegerValue = RightNode.IntegerValue;
          break;
        case Type.Long:
          variable.LongValue = RightNode.LongValue;
          break;
        case Type.Float:
          variable.FloatValue = RightNode.FloatValue;
          break;
        case Type.Double:
          variable.DoubleValue = RightNode.DoubleValue;
          break;
        case Type.Decimal:
          variable.DecimalValue = RightNode.DecimalValue;
          break;
        case Type.Boolean:
          if (RightNode.ValueType.IsFloat())
          {
            throw CreateRuntimeException("A float value cannot be assigned to a boolean variable");
          }

          if (RightNode.IsNumeric)
          {
            throw CreateRuntimeException("A numeric value cannot be assigned to a boolean variable");
          }

          variable.BooleanValue = RightNode.BooleanValue;
          break;
        case Type.String:
          variable.StringValue = RightNode.StringValue;
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(variable.Type), variable.Type, "Unsupported variable type");
      }
    }
  }
}