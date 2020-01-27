using System;
using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Parser;

namespace EpsilonScript.AST
{
  public class SignOperator : Node
  {
    private Node _childNode;
    private ElementType _operationType;

    public override bool IsConstant => _childNode.IsConstant;

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IDictionary<string, VariableValue> variables,
      IDictionary<string, CustomFunctionOverload> functions)
    {
      if (!rpnStack.TryPop(out _childNode))
      {
        throw new ParserException(element.Token, "Cannot find value to perform sign operation on");
      }

      _operationType = element.Type;
    }

    public override void Execute()
    {
      _childNode.Execute();

      ValueType = _childNode.ValueType;
      if (ValueType != ValueType.Integer && ValueType != ValueType.Float)
      {
        throw new RuntimeException("Sign of a non-numeric value cannot be changed");
      }

      switch (ValueType)
      {
        case ValueType.Integer:
          IntegerValue = _operationType switch
          {
            ElementType.PositiveOperator => _childNode.IntegerValue,
            ElementType.NegativeOperator => -_childNode.IntegerValue,
            _ => throw new ArgumentOutOfRangeException(nameof(_operationType), _operationType,
              "Unsupported operation type for sign change")
          };
          FloatValue = IntegerValue;
          BooleanValue = IntegerValue != 0;
          break;
        case ValueType.Float:
          FloatValue = _operationType switch
          {
            ElementType.PositiveOperator => _childNode.FloatValue,
            ElementType.NegativeOperator => -_childNode.FloatValue,
            _ => throw new ArgumentOutOfRangeException(nameof(_operationType), _operationType,
              "Unsupported operation type for sign change")
          };
          IntegerValue = (int) FloatValue;
          BooleanValue = IntegerValue != 0;
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(ValueType), ValueType,
            "Unsupported value type for sign change");
      }
    }
  }
}