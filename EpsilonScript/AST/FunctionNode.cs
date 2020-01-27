using System;
using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Parser;

namespace EpsilonScript.AST
{
  public class FunctionNode : Node
  {
    public override bool IsConstant => _functionOverload.IsConstant && AreParametersConstant;

    private CustomFunctionOverload _functionOverload;
    private Type[] _parameterTypes;

    private List<Node> _parameters;

    private bool AreParametersConstant
    {
      get
      {
        foreach (var node in _parameters)
        {
          if (!node.IsConstant)
          {
            return false;
          }
        }

        return true;
      }
    }

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IDictionary<string, VariableValue> variables,
      IDictionary<string, CustomFunctionOverload> functions)
    {
      if (!functions.TryGetValue(element.Token.Text, out _functionOverload))
      {
        throw new ParserException(element.Token, $"Undefined function: {element.Token.Text}");
      }

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
          _parameters = new List<Node>();
          _parameters.Add(childNode);
          _parameterTypes = new Type[1];
          break;
        case ValueType.Tuple:
          _parameters = childNode.TupleValue;
          _parameterTypes = new Type[_parameters.Count];
          break;
        case ValueType.Null:
          _parameterTypes = Array.Empty<Type>();
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(childNode.ValueType), childNode.ValueType,
            "Unsupported child not value type");
      }
    }

    public override void Execute()
    {
      // Execute each parameter and populate type information for function invocation
      // Parameter type is undefined until executed, due to the fact that a variable type may change after compilation
      for (var i = 0; i < _parameters.Count; ++i)
      {
        var parameter = _parameters[i];
        parameter.Execute();
        _parameterTypes[i] = parameter.ValueType switch
        {
          ValueType.Integer => Type.Integer,
          ValueType.Float => Type.Float,
          ValueType.Boolean => Type.Boolean,
          _ => throw new ArgumentOutOfRangeException(nameof(_parameters), parameter.ValueType,
            "Unsupported parameter value type")
        };
      }

      var function = _functionOverload.Find(_parameterTypes);
      if (function == null)
      {
        throw new RuntimeException("A function with given type signature is undefined");
      }

      ValueType = function.ReturnType switch
      {
        Type.Integer => ValueType.Integer,
        Type.Float => ValueType.Float,
        _ => throw new ArgumentOutOfRangeException(nameof(function.ReturnType), function.ReturnType,
          "Unsupported function return type")
      };

      switch (ValueType)
      {
        case ValueType.Integer:
          IntegerValue = function.ExecuteInt(_parameters);
          FloatValue = IntegerValue;
          BooleanValue = IntegerValue != 0;
          break;
        case ValueType.Float:
          FloatValue = function.ExecuteFloat(_parameters);
          IntegerValue = (int) FloatValue;
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(ValueType), ValueType, "Unsupported function return type");
      }
    }
  }
}