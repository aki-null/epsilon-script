using System;
using System.Collections.Generic;

namespace EpsilonScript.Function
{
  /// <summary>
  /// Custom function parameter type information is stored as a tree.
  /// This is because the exact function to execute depends on the parameter types, so we need to have a method to
  /// discover various overload patterns from left to right.
  /// </summary>
  internal class CustomFunctionOverloadNode
  {
    private readonly Compiler.FloatPrecision _floatPrecision;
    private readonly Compiler.IntegerPrecision _intPrecision;

    private CustomFunction LeafFunction { get; set; }

    private CustomFunctionOverloadNode _integerNode;
    private CustomFunctionOverloadNode _floatNode;
    private CustomFunctionOverloadNode _booleanNode;
    private CustomFunctionOverloadNode _stringNode;

    public CustomFunctionOverloadNode(Compiler.IntegerPrecision intPrecision, Compiler.FloatPrecision floatPrecision)
    {
      _floatPrecision = floatPrecision;
      _intPrecision = intPrecision;
    }

    public CustomFunction Find(PackedParameterTypes packedTypes, int index = 0)
    {
      if (index >= packedTypes.Count)
      {
        return LeafFunction;
      }

      var paramType = packedTypes.GetTypeAt(index);

      ++index;

      // Undefined represents a wildcard (variable) - return first matching function
      if (paramType == ExtendedType.Undefined)
      {
        var candidate = _integerNode?.Find(packedTypes, index);
        if (candidate != null) return candidate;

        candidate = _floatNode?.Find(packedTypes, index);
        if (candidate != null) return candidate;

        candidate = _booleanNode?.Find(packedTypes, index);
        if (candidate != null) return candidate;

        candidate = _stringNode?.Find(packedTypes, index);
        if (candidate != null) return candidate;

        return null;
      }

      var nextNode = paramType switch
      {
        ExtendedType.Integer => _integerNode,
        ExtendedType.Long => _integerNode,
        ExtendedType.Float => _floatNode,
        ExtendedType.Double => _floatNode,
        ExtendedType.Decimal => _floatNode,
        ExtendedType.Boolean => _booleanNode,
        ExtendedType.String => _stringNode,
        _ => null
      };

      var result = nextNode?.Find(packedTypes, index);
      if (result != null)
      {
        return result;
      }

      // Fallback: try converting integer types to the configured float type
      if (paramType == ExtendedType.Integer || paramType == ExtendedType.Long)
      {
        return _floatNode?.Find(packedTypes, index);
      }

      return null;
    }

    public void Build(CustomFunction function, int index = 0)
    {
      var paramTypes = function.ParameterTypes;
      if (index >= paramTypes.Length)
      {
        if (LeafFunction != null)
        {
          throw new RuntimeException(
            $"The custom function with same name and same parameter types is already defined: {function}");
        }

        LeafFunction = function;
        return;
      }

      var paramType = paramTypes[index];

      // Map function parameter type to node based on configured precision
      var nextNode = paramType switch
      {
        Type.Integer when _intPrecision == Compiler.IntegerPrecision.Integer => _integerNode ??=
          new CustomFunctionOverloadNode(_intPrecision, _floatPrecision),
        Type.Long when _intPrecision == Compiler.IntegerPrecision.Long => _integerNode ??=
          new CustomFunctionOverloadNode(_intPrecision, _floatPrecision),

        Type.Float when _floatPrecision == Compiler.FloatPrecision.Float => _floatNode ??=
          new CustomFunctionOverloadNode(_intPrecision, _floatPrecision),
        Type.Double when _floatPrecision == Compiler.FloatPrecision.Double => _floatNode ??=
          new CustomFunctionOverloadNode(_intPrecision, _floatPrecision),
        Type.Decimal when _floatPrecision == Compiler.FloatPrecision.Decimal => _floatNode ??=
          new CustomFunctionOverloadNode(_intPrecision, _floatPrecision),

        Type.Boolean => _booleanNode ??= new CustomFunctionOverloadNode(_intPrecision, _floatPrecision),
        Type.String => _stringNode ??= new CustomFunctionOverloadNode(_intPrecision, _floatPrecision),

        // Error: function parameter type doesn't match compiler precision
        Type.Integer => throw new ArgumentException(
          $"Cannot add function with Integer parameter to compiler with {_intPrecision} precision: {function}"),
        Type.Long => throw new ArgumentException(
          $"Cannot add function with Long parameter to compiler with {_intPrecision} precision: {function}"),
        Type.Float => throw new ArgumentException(
          $"Cannot add function with Float parameter to compiler with {_floatPrecision} precision: {function}"),
        Type.Double => throw new ArgumentException(
          $"Cannot add function with Double parameter to compiler with {_floatPrecision} precision: {function}"),
        Type.Decimal => throw new ArgumentException(
          $"Cannot add function with Decimal parameter to compiler with {_floatPrecision} precision: {function}"),

        _ => throw new ArgumentOutOfRangeException(nameof(paramTypes), paramType,
          $"Unsupported parameter type at position {index} in function: {function}")
      };

      nextNode.Build(function, index + 1);
    }

    public void CollectAllFunctions(List<CustomFunction> functions)
    {
      if (LeafFunction != null)
      {
        functions.Add(LeafFunction);
      }

      _integerNode?.CollectAllFunctions(functions);
      _floatNode?.CollectAllFunctions(functions);
      _booleanNode?.CollectAllFunctions(functions);
      _stringNode?.CollectAllFunctions(functions);
    }
  }
}