using System;
using System.Collections.Generic;
using EpsilonScript.Parser;

namespace EpsilonScript.AST
{
  public class FunctionNode : Node
  {
    public override bool IsConstant => _function.IsConstant && AreArgumentsConstant;

    private CustomFunction _function;

    private List<Node> _arguments;

    private bool AreArgumentsConstant
    {
      get
      {
        foreach (var node in _arguments)
        {
          if (!node.IsConstant)
          {
            return false;
          }
        }

        return true;
      }
    }

    public override void Build(Stack<Node> rpnStack, Element element, IDictionary<string, VariableValue> variables,
      IDictionary<string, CustomFunction> functions)
    {
      if (!functions.TryGetValue(element.Token.Text, out _function))
      {
        throw new ParserException(element.Token, $"Undefined function: {element.Token.Text}");
      }

      ValueType = _function.ReturnType switch
      {
        Type.Integer => ValueType.Integer,
        Type.Float => ValueType.Float,
        _ => throw new ArgumentOutOfRangeException(nameof(_function.ReturnType), _function.ReturnType,
          "Unsupported function return type")
      };

      if (!rpnStack.TryPop(out var childNode))
      {
        throw new ParserException(element.Token, $"Cannot find parameters for calling function: {element.Token.Text}");
      }

      switch (childNode.ValueType)
      {
        case ValueType.Boolean:
        case ValueType.Float:
        case ValueType.Integer:
        case ValueType.Undefined:
          _arguments = new List<Node>();
          _arguments.Add(childNode);
          break;
        case ValueType.Tuple:
          _arguments = childNode.TupleValue;
          break;
        case ValueType.Null:
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(childNode.ValueType), childNode.ValueType,
            "Unsupported child not value type");
      }
    }

    public override void Execute()
    {
      foreach (var argument in _arguments)
      {
        argument.Execute();
      }

      switch (ValueType)
      {
        case ValueType.Integer:
          IntegerValue = _function.ExecuteInt(_arguments);
          FloatValue = IntegerValue;
          BooleanValue = IntegerValue != 0;
          break;
        case ValueType.Float:
          FloatValue = _function.ExecuteFloat(_arguments);
          IntegerValue = (int) FloatValue;
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(ValueType), ValueType, "Unsupported function return type");
      }
    }
  }
}