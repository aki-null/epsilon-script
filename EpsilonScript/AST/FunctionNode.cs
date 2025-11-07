using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  internal sealed class FunctionNode : Node
  {
    public override bool IsPrecomputable => _functionOverload.IsDeterministic && AreParametersPrecomputable;

    private CustomFunctionOverload _functionOverload;
    private List<Node> _parameters;
    private IVariableContainer _variables;

    // Cache resolved function using packed types for fast comparison
    private CustomFunction _cachedFunction;
    private PackedParameterTypes _cachedPackedTypes;
    private int _cachedVersion;

    private bool AreParametersPrecomputable
    {
      get
      {
        foreach (var node in _parameters)
        {
          if (!node.IsPrecomputable)
          {
            return false;
          }
        }

        return true;
      }
    }

    protected override void BuildCore(Stack<Node> rpnStack, Element element, CompilerContext context,
      Compiler.Options options, IVariableContainer variables)
    {
      // Unfortunately function name string needs to be allocated here to make a dictionary lookup
      VariableId functionName = element.Token.Text.ToString();

      // Store compile-time variables for contextual functions
      _variables = variables;

      if (!context.Functions.TryGetValue(functionName, out _functionOverload))
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
          break;
        case ExtendedType.Null:
          _parameters = new List<Node>();
          break;
        default:
          _parameters = new List<Node> { childNode };
          break;
      }

      // Validate parameter count once at build time (PackedParameterTypes supports max 7)
      if (_parameters.Count > 7)
      {
        throw new ParserException(element.Token,
          $"Function {functionName} has too many parameters (max 7): {_parameters.Count}");
      }
    }

    public override void Execute(IVariableContainer variablesOverride)
    {
      // Build packed parameter types incrementally as we execute parameters
      var packedTypes = new PackedParameterTypes();
      foreach (var parameter in _parameters)
      {
        parameter.Execute(variablesOverride);
        packedTypes.AddType(parameter.ValueType);
      }

      // Fast path: Check cache for previously resolved function
      if (_cachedFunction != null && _cachedPackedTypes == packedTypes && _cachedVersion == _functionOverload.Version)
      {
        ExecuteFunction(variablesOverride);
        return;
      }

      // Slow path: Resolve function
      var function = _functionOverload.Find(packedTypes);
      if (function == null)
      {
        throw CreateRuntimeException(CreateFunctionErrorMessage(packedTypes));
      }

      // Cache resolved function for performance
      _cachedFunction = function;
      _cachedPackedTypes = packedTypes;
      _cachedVersion = _functionOverload.Version;
      ValueType = (ExtendedType)function.ReturnType;
      ExecuteFunction(variablesOverride);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteFunction(IVariableContainer variablesOverride)
    {
      // Check if function requires context variables
      if (_cachedFunction.IsContextual)
      {
        ExecuteFunctionWithContext(variablesOverride);
      }
      else
      {
        ExecuteFunctionWithoutContext();
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteFunctionWithoutContext()
    {
      switch (ValueType)
      {
        case ExtendedType.Integer:
          IntegerValue = _cachedFunction.ExecuteInt(_parameters);
          break;
        case ExtendedType.Long:
          LongValue = _cachedFunction.ExecuteLong(_parameters);
          break;
        case ExtendedType.Float:
          FloatValue = _cachedFunction.ExecuteFloat(_parameters);
          break;
        case ExtendedType.Double:
          DoubleValue = _cachedFunction.ExecuteDouble(_parameters);
          break;
        case ExtendedType.Decimal:
          DecimalValue = _cachedFunction.ExecuteDecimal(_parameters);
          break;
        case ExtendedType.String:
          StringValue = _cachedFunction.ExecuteString(_parameters);
          break;
        case ExtendedType.Boolean:
          BooleanValue = _cachedFunction.ExecuteBool(_parameters);
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(ValueType), ValueType,
            $"Unsupported return type for function: {_cachedFunction.Name}");
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteFunctionWithContext(IVariableContainer variablesOverride)
    {
      var context = new VariableContextAdapter(variablesOverride, _variables);

      switch (ValueType)
      {
        case ExtendedType.Integer:
          IntegerValue = _cachedFunction.ExecuteInt(context, _parameters);
          break;
        case ExtendedType.Long:
          LongValue = _cachedFunction.ExecuteLong(context, _parameters);
          break;
        case ExtendedType.Float:
          FloatValue = _cachedFunction.ExecuteFloat(context, _parameters);
          break;
        case ExtendedType.Double:
          DoubleValue = _cachedFunction.ExecuteDouble(context, _parameters);
          break;
        case ExtendedType.Decimal:
          DecimalValue = _cachedFunction.ExecuteDecimal(context, _parameters);
          break;
        case ExtendedType.String:
          StringValue = _cachedFunction.ExecuteString(context, _parameters);
          break;
        case ExtendedType.Boolean:
          BooleanValue = _cachedFunction.ExecuteBool(context, _parameters);
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(ValueType), ValueType,
            $"Unsupported return type for function: {_cachedFunction.Name}");
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
      if (IsPrecomputable)
      {
        Execute(null);
        return CreateValueNode();
      }

      return this;
    }

    public override void Validate()
    {
      // First, recursively validate all parameter nodes
      if (_parameters != null)
      {
        foreach (var param in _parameters)
        {
          param?.Validate();
        }
      }

      // Then validate this function's signature if possible
      ValidateSignature();
    }

    private void ValidateSignature()
    {
      // Build packed types
      var packedTypes = new PackedParameterTypes();
      foreach (var param in _parameters)
      {
        packedTypes.AddType(param.ValueType);
      }

      // Returns a function (exact or candidate) if match possible, null if validation fails
      var function = _functionOverload.Find(packedTypes);

      if (function != null)
      {
        // Match found - cache it (might be exact match or first candidate with Undefined params)
        _cachedFunction = function;
        _cachedPackedTypes = packedTypes;
        ValueType = (ExtendedType)function.ReturnType;
      }
      else
      {
        // No match possible - validation failed
        throw new ParserException(Location, CreateFunctionErrorMessage(packedTypes));
      }
    }

    private string CreateFunctionErrorMessage(PackedParameterTypes packedTypes)
    {
      var availableOverloads = _functionOverload.GetAllOverloads();
      var overloadList = string.Join(", ", availableOverloads);
      var message = $"No overload of function '{_functionOverload.Name}' matches parameter types: ({packedTypes}).\n" +
                    $"Available overloads: {overloadList}";
      return message;
    }

    public override void ConfigureNoAlloc()
    {
      if (_parameters == null) return;

      foreach (var param in _parameters)
      {
        param?.ConfigureNoAlloc();
      }
    }
  }
}