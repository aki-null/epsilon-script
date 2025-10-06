using System;
using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  public class ArithmeticNode : Node
  {
    private Node _leftNode;
    private Node _rightNode;
    private ElementType _operator;
    private ValueType _configuredIntegerType;
    private ValueType _configuredFloatType;

    public override bool IsConstant => _leftNode.IsConstant && _rightNode.IsConstant;

    private static string GetOperatorName(ElementType op)
    {
      return op switch
      {
        ElementType.AddOperator => "addition",
        ElementType.SubtractOperator => "subtraction",
        ElementType.MultiplyOperator => "multiplication",
        ElementType.DivideOperator => "division",
        ElementType.ModuloOperator => "modulo",
        _ => "unknown operation"
      };
    }

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IVariableContainer variables, IDictionary<VariableId, CustomFunctionOverload> functions,
      Compiler.IntegerPrecision intPrecision, Compiler.FloatPrecision floatPrecision)
    {
      if (!rpnStack.TryPop(out _rightNode) || !rpnStack.TryPop(out _leftNode))
      {
        throw new ParserException(element.Token, "Cannot find values to perform arithmetic operation on");
      }

      _operator = element.Type;

      // Store configured types for optimized type promotion
      _configuredIntegerType = intPrecision == Compiler.IntegerPrecision.Integer ? ValueType.Integer : ValueType.Long;
      _configuredFloatType = floatPrecision switch
      {
        Compiler.FloatPrecision.Float => ValueType.Float,
        Compiler.FloatPrecision.Double => ValueType.Double,
        Compiler.FloatPrecision.Decimal => ValueType.Decimal,
        _ => ValueType.Float
      };
    }

    private ValueType PromoteType(ValueType left, ValueType right)
    {
      if (left == ValueType.String || right == ValueType.String)
        return ValueType.String;

      // Since precision is fixed per compiler, only two numeric types exist:
      // the configured integer type and the configured float type
      if (left == _configuredFloatType || right == _configuredFloatType)
        return _configuredFloatType;

      return _configuredIntegerType;
    }

    public override void Execute(IVariableContainer variablesOverride)
    {
      _leftNode.Execute(variablesOverride);
      _rightNode.Execute(variablesOverride);

      if ((!_leftNode.IsNumeric || !_rightNode.IsNumeric) && _leftNode.ValueType != ValueType.String)
      {
        if (_leftNode.ValueType == ValueType.Boolean || _rightNode.ValueType == ValueType.Boolean)
        {
          throw new RuntimeException(
            $"Boolean values cannot be used in arithmetic operations ({GetOperatorName(_operator)})");
        }

        throw new RuntimeException("An arithmetic operation can only be performed on numeric values");
      }

      ValueType = PromoteType(_leftNode.ValueType, _rightNode.ValueType);

      var targetType = ValueType;
      // Consolidated arithmetic calculation using switch expression for better performance
      // Single dispatch point reduces branch mispredictions and method call overhead
      switch (targetType)
      {
        case ValueType.Integer:
          var leftInt = _leftNode.IntegerValue;
          var rightInt = _rightNode.IntegerValue;
          IntegerValue = _operator switch
          {
            ElementType.AddOperator => leftInt + rightInt,
            ElementType.SubtractOperator => leftInt - rightInt,
            ElementType.MultiplyOperator => leftInt * rightInt,
            ElementType.DivideOperator => rightInt == 0
              ? throw new DivideByZeroException("Division by zero")
              : leftInt / rightInt,
            ElementType.ModuloOperator => rightInt == 0
              ? throw new DivideByZeroException("Modulo by zero")
              : leftInt % rightInt,
            _ => throw new ArgumentOutOfRangeException(nameof(_operator), _operator, "Unsupported operator type")
          };
          break;

        case ValueType.Long:
          var leftLong = _leftNode.LongValue;
          var rightLong = _rightNode.LongValue;
          LongValue = _operator switch
          {
            ElementType.AddOperator => leftLong + rightLong,
            ElementType.SubtractOperator => leftLong - rightLong,
            ElementType.MultiplyOperator => leftLong * rightLong,
            ElementType.DivideOperator => rightLong == 0
              ? throw new DivideByZeroException("Division by zero")
              : leftLong / rightLong,
            ElementType.ModuloOperator => rightLong == 0
              ? throw new DivideByZeroException("Modulo by zero")
              : leftLong % rightLong,
            _ => throw new ArgumentOutOfRangeException(nameof(_operator), _operator, "Unsupported operator type")
          };
          break;

        case ValueType.Float:
          var leftFloat = _leftNode.FloatValue;
          var rightFloat = _rightNode.FloatValue;
          FloatValue = _operator switch
          {
            ElementType.AddOperator => leftFloat + rightFloat,
            ElementType.SubtractOperator => leftFloat - rightFloat,
            ElementType.MultiplyOperator => leftFloat * rightFloat,
            ElementType.DivideOperator => rightFloat == 0.0f
              ? throw new DivideByZeroException("Division by zero")
              : leftFloat / rightFloat,
            ElementType.ModuloOperator => rightFloat == 0.0f
              ? throw new DivideByZeroException("Modulo by zero")
              : leftFloat % rightFloat,
            _ => throw new ArgumentOutOfRangeException(nameof(_operator), _operator, "Unsupported operator type")
          };
          break;

        case ValueType.Double:
          var leftDouble = _leftNode.DoubleValue;
          var rightDouble = _rightNode.DoubleValue;
          DoubleValue = _operator switch
          {
            ElementType.AddOperator => leftDouble + rightDouble,
            ElementType.SubtractOperator => leftDouble - rightDouble,
            ElementType.MultiplyOperator => leftDouble * rightDouble,
            ElementType.DivideOperator => rightDouble == 0.0
              ? throw new DivideByZeroException("Division by zero")
              : leftDouble / rightDouble,
            ElementType.ModuloOperator => rightDouble == 0.0
              ? throw new DivideByZeroException("Modulo by zero")
              : leftDouble % rightDouble,
            _ => throw new ArgumentOutOfRangeException(nameof(_operator), _operator, "Unsupported operator type")
          };
          break;

        case ValueType.Decimal:
          var leftDecimal = _leftNode.DecimalValue;
          var rightDecimal = _rightNode.DecimalValue;
          DecimalValue = _operator switch
          {
            ElementType.AddOperator => leftDecimal + rightDecimal,
            ElementType.SubtractOperator => leftDecimal - rightDecimal,
            ElementType.MultiplyOperator => leftDecimal * rightDecimal,
            ElementType.DivideOperator => rightDecimal == 0m
              ? throw new DivideByZeroException("Division by zero")
              : leftDecimal / rightDecimal,
            ElementType.ModuloOperator => rightDecimal == 0m
              ? throw new DivideByZeroException("Modulo by zero")
              : leftDecimal % rightDecimal,
            _ => throw new ArgumentOutOfRangeException(nameof(_operator), _operator, "Unsupported operator type")
          };
          break;

        case ValueType.String:
          if (_operator == ElementType.AddOperator)
          {
            // The left node is guaranteed to be a string node
            StringValue = _leftNode.StringValue + _rightNode.ToString();
          }
          else
          {
            throw new RuntimeException(
              $"String operations only support concatenation (+), not {GetOperatorName(_operator)}");
          }

          break;

        default:
          throw new ArgumentOutOfRangeException(nameof(targetType), targetType, "Unsupported value type");
      }
    }

    public override Node Optimize()
    {
      if (IsConstant)
      {
        Execute(null);
        return CreateValueNode();
      }

      _leftNode = _leftNode.Optimize();
      _rightNode = _rightNode.Optimize();
      return this;
    }
  }
}