using System;
using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  internal class FunctionNode : Node
  {
    public override bool IsConstant => _functionOverload.IsConstant && AreParametersConstant;

    private CustomFunctionOverload _functionOverload;
    private ExtendedType[] _parameterTypes;

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
      IVariableContainer variables, IDictionary<VariableId, CustomFunctionOverload> functions,
      Compiler.IntegerPrecision intPrecision, Compiler.FloatPrecision floatPrecision)
    {
      // Unfortunately function name string needs to be allocated here to make a dictionary lookup
      VariableId functionName = element.Token.Text.ToString();

      if (!functions.TryGetValue(functionName, out _functionOverload))
      {
        throw new ParserException(element.Token, $"Undefined function: {functionName}");
      }

      if (!rpnStack.TryPop(out var childNode))
      {
        throw new ParserException(element.Token,
          $"Cannot find parameters for calling function: {functionName}");
      }

      switch (childNode.ValueType)
      {
        case ExtendedType.Tuple:
          _parameters = childNode.TupleValue;
          _parameterTypes = new ExtendedType[_parameters.Count];
          break;
        case ExtendedType.Null:
          _parameters = new List<Node>();
          _parameterTypes = Array.Empty<ExtendedType>();
          break;
        default:
          _parameters = new List<Node> { childNode };
          _parameterTypes = new ExtendedType[1];
          break;
      }
    }

    public override void Execute(IVariableContainer variablesOverride)
    {
      // Execute each parameter and populate type information for function invocation
      // Parameter type is undefined until executed, due to the fact that a variable type may change after compilation
      for (var i = 0; i < _parameters.Count; ++i)
      {
        var parameter = _parameters[i];
        parameter.Execute(variablesOverride);
        _parameterTypes[i] = parameter.ValueType;
      }

      var function = _functionOverload.Find(_parameterTypes);
      if (function == null)
      {
        throw new RuntimeException("A function with given type signature is undefined");
      }

      ValueType = (ExtendedType)function.ReturnType;

      switch (ValueType)
      {
        case ExtendedType.Integer:
          IntegerValue = function.ExecuteInt(_parameters);
          break;
        case ExtendedType.Long:
          LongValue = function.ExecuteLong(_parameters);
          break;
        case ExtendedType.Float:
          FloatValue = function.ExecuteFloat(_parameters);
          break;
        case ExtendedType.Double:
          DoubleValue = function.ExecuteDouble(_parameters);
          break;
        case ExtendedType.Decimal:
          DecimalValue = function.ExecuteDecimal(_parameters);
          break;
        case ExtendedType.String:
          StringValue = function.ExecuteString(_parameters);
          break;
        case ExtendedType.Boolean:
          BooleanValue = function.ExecuteBool(_parameters);
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(ValueType), ValueType,
            "Unsupported function return type");
      }
    }

    public override Node Optimize()
    {
      // Optimize child parameter nodes first
      for (var i = 0; i < _parameters.Count; ++i)
      {
        _parameters[i] = _parameters[i].Optimize();
      }

      // If function is constant and all parameters are constant, evaluate at compile time
      if (IsConstant)
      {
        Execute(null);
        return CreateValueNode();
      }

      return this;
    }
  }
}