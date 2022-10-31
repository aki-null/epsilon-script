using System;
using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  public class AssignmentNode : Node
  {
    private Node _leftNode;
    private Node _rightNode;
    private ElementType _assignmentType;

    public override bool IsConstant => false;

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IVariableContainer variables, IDictionary<uint, CustomFunctionOverload> functions)
    {
      if ((options & Compiler.Options.Immutable) == Compiler.Options.Immutable)
      {
        throw new ParserException(element.Token, "An assignment operator cannot be used for an immutable script");
      }

      _assignmentType = element.Type;

      if (!rpnStack.TryPop(out _rightNode) || !rpnStack.TryPop(out _leftNode))
      {
        throw new ParserException(element.Token, "Cannot find elements to perform assignment operation on");
      }
    }

    public override void Execute(IVariableContainer variablesOverride)
    {
      _leftNode.Execute(variablesOverride);
      _rightNode.Execute(variablesOverride);

      if (_leftNode.Variable == null)
      {
        throw new RuntimeException("A left hand side of an assignment operator must be a variable");
      }

      var variable = _leftNode.Variable;

      switch (_assignmentType)
      {
        case ElementType.AssignmentOperator:
          switch (variable.Type)
          {
            case Type.Integer:
              variable.IntegerValue = _rightNode.IntegerValue;
              break;
            case Type.Float:
              variable.FloatValue = _rightNode.FloatValue;
              break;
            case Type.Boolean:
              if (_rightNode.ValueType == ValueType.Float)
              {
                throw new RuntimeException("A float value cannot be assigned to a boolean variable");
              }

              variable.BooleanValue = _rightNode.BooleanValue;
              break;
            case Type.String:
              variable.StringValue = _rightNode.StringValue;
              break;
            default:
              throw new ArgumentOutOfRangeException(nameof(variable.Type), variable.Type, "Unsupported variable type");
          }

          break;
        case ElementType.AssignmentAddOperator:
          if (!_rightNode.IsNumeric)
          {
            throw new RuntimeException("An arithmetic operation can only be performed on a numeric value");
          }

          switch (variable.Type)
          {
            case Type.Integer:
              variable.IntegerValue += _rightNode.IntegerValue;
              break;
            case Type.Float:
              variable.FloatValue += _rightNode.FloatValue;
              break;
            default:
              throw new ArgumentOutOfRangeException(nameof(variable.Type), variable.Type, "Unsupported variable type");
          }

          break;
        case ElementType.AssignmentSubtractOperator:
          if (!_rightNode.IsNumeric)
          {
            throw new RuntimeException("An arithmetic operation can only be performed on a numeric value");
          }

          switch (variable.Type)
          {
            case Type.Integer:
              variable.IntegerValue -= _rightNode.IntegerValue;
              break;
            case Type.Float:
              variable.FloatValue -= _rightNode.FloatValue;
              break;
            default:
              throw new ArgumentOutOfRangeException(nameof(variable.Type), variable.Type, "Unsupported variable type");
          }

          break;
        case ElementType.AssignmentMultiplyOperator:
          if (!_rightNode.IsNumeric)
          {
            throw new RuntimeException("An arithmetic operation can only be performed on a numeric value");
          }

          switch (variable.Type)
          {
            case Type.Integer:
              variable.IntegerValue *= _rightNode.IntegerValue;
              break;
            case Type.Float:
              variable.FloatValue *= _rightNode.FloatValue;
              break;
            default:
              throw new ArgumentOutOfRangeException(nameof(variable.Type), variable.Type, "Unsupported variable type");
          }

          break;
        case ElementType.AssignmentDivideOperator:
          if (!_rightNode.IsNumeric)
          {
            throw new RuntimeException("An arithmetic operation can only be performed on a numeric value");
          }

          switch (variable.Type)
          {
            case Type.Integer:
              variable.IntegerValue /= _rightNode.IntegerValue;
              break;
            case Type.Float:
              variable.FloatValue /= _rightNode.FloatValue;
              break;
            default:
              throw new ArgumentOutOfRangeException(nameof(variable.Type), variable.Type, "Unsupported variable type");
          }

          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(_assignmentType), _assignmentType,
            "Unsupported assignment type");
      }

      switch (variable.Type)
      {
        case Type.Integer:
          ValueType = ValueType.Integer;
          IntegerValue = variable.IntegerValue;
          FloatValue = variable.FloatValue;
          BooleanValue = variable.BooleanValue;
          break;
        case Type.Float:
          ValueType = ValueType.Float;
          IntegerValue = variable.IntegerValue;
          FloatValue = variable.FloatValue;
          break;
        case Type.Boolean:
          ValueType = ValueType.Boolean;
          IntegerValue = variable.IntegerValue;
          BooleanValue = variable.BooleanValue;
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(variable.Type), variable.Type, "Unsupported variable type");
      }
    }
  }
}