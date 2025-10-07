using System;
using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  public class SignOperator : Node
  {
    private Node _childNode;
    private ElementType _operationType;

    public override bool IsConstant => _childNode.IsConstant;

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IVariableContainer variables, IDictionary<VariableId, CustomFunctionOverload> functions,
      Compiler.IntegerPrecision intPrecision, Compiler.FloatPrecision floatPrecision)
    {
      if (!rpnStack.TryPop(out _childNode))
      {
        throw new ParserException(element.Token, "Cannot find value to perform sign operation on");
      }

      _operationType = element.Type;
    }

    public override void Execute(IVariableContainer variablesOverride)
    {
      _childNode.Execute(variablesOverride);

      if (!_childNode.IsNumeric)
      {
        throw new RuntimeException("Sign of a non-numeric value cannot be changed");
      }

      switch (_childNode.ValueType)
      {
        case Type.Integer:
          switch (_operationType)
          {
            case ElementType.PositiveOperator:
              IntegerValue = _childNode.IntegerValue;
              break;
            case ElementType.NegativeOperator:
              IntegerValue = -_childNode.IntegerValue;
              break;
            default:
              throw new ArgumentOutOfRangeException(nameof(_operationType), _operationType,
                "Unsupported operation type for sign change");
          }

          break;
        case Type.Long:
          switch (_operationType)
          {
            case ElementType.PositiveOperator:
              LongValue = _childNode.LongValue;
              break;
            case ElementType.NegativeOperator:
              LongValue = -_childNode.LongValue;
              break;
            default:
              throw new ArgumentOutOfRangeException(nameof(_operationType), _operationType,
                "Unsupported operation type for sign change");
          }

          break;
        case Type.Float:
          switch (_operationType)
          {
            case ElementType.PositiveOperator:
              FloatValue = _childNode.FloatValue;
              break;
            case ElementType.NegativeOperator:
              FloatValue = -_childNode.FloatValue;
              break;
            default:
              throw new ArgumentOutOfRangeException(nameof(_operationType), _operationType,
                "Unsupported operation type for sign change");
          }

          break;
        case Type.Double:
          switch (_operationType)
          {
            case ElementType.PositiveOperator:
              DoubleValue = _childNode.DoubleValue;
              break;
            case ElementType.NegativeOperator:
              DoubleValue = -_childNode.DoubleValue;
              break;
            default:
              throw new ArgumentOutOfRangeException(nameof(_operationType), _operationType,
                "Unsupported operation type for sign change");
          }

          break;
        case Type.Decimal:
          switch (_operationType)
          {
            case ElementType.PositiveOperator:
              DecimalValue = _childNode.DecimalValue;
              break;
            case ElementType.NegativeOperator:
              DecimalValue = -_childNode.DecimalValue;
              break;
            default:
              throw new ArgumentOutOfRangeException(nameof(_operationType), _operationType,
                "Unsupported operation type for sign change");
          }

          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(_childNode.ValueType), _childNode.ValueType,
            "Unsupported value type for sign change");
      }
    }

    public override Node Optimize()
    {
      if (IsConstant)
      {
        Execute(null);
        return CreateValueNode();
      }

      if (_operationType == ElementType.PositiveOperator)
      {
        // Unary positive operator is a no-op, just return the optimized child
        return _childNode.Optimize();
      }

      _childNode = _childNode.Optimize();
      return this;
    }
  }
}