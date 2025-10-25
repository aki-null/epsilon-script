using System;

namespace EpsilonScript.Function
{
  /// <summary>
  /// Custom function parameter type information is stored as a tree.
  /// This is because the exact function to execute depends on the parameter types, so we need to have a method to
  /// discover various overload patterns from left to right.
  /// </summary>
  internal class CustomFunctionOverloadNode
  {
    private CustomFunction LeafFunction { get; set; }

    private CustomFunctionOverloadNode _integerNode;
    private CustomFunctionOverloadNode _longNode;
    private CustomFunctionOverloadNode _floatNode;
    private CustomFunctionOverloadNode _doubleNode;
    private CustomFunctionOverloadNode _decimalNode;
    private CustomFunctionOverloadNode _booleanNode;
    private CustomFunctionOverloadNode _stringNode;

    public CustomFunction Find(PackedParameterTypes packedTypes, Compiler.FloatPrecision configuredFloatType,
      int index = 0)
    {
      if (index >= packedTypes.Count)
      {
        return LeafFunction;
      }

      var paramType = packedTypes.GetTypeAt(index);
      var nextNode = paramType switch
      {
        ExtendedType.Integer => _integerNode,
        ExtendedType.Long => _longNode,
        ExtendedType.Float => _floatNode,
        ExtendedType.Double => _doubleNode,
        ExtendedType.Decimal => _decimalNode,
        ExtendedType.Boolean => _booleanNode,
        ExtendedType.String => _stringNode,
        _ => null
      };

      var result = nextNode?.Find(packedTypes, configuredFloatType, index + 1);
      if (result != null)
      {
        return result;
      }

      // Fallback: try converting integer types to the configured float type
      // Since compiler precision is fixed, only one float type exists per script
      if (paramType == ExtendedType.Integer || paramType == ExtendedType.Long)
      {
        var fallbackNode = configuredFloatType switch
        {
          Compiler.FloatPrecision.Float => _floatNode,
          Compiler.FloatPrecision.Double => _doubleNode,
          Compiler.FloatPrecision.Decimal => _decimalNode,
          _ => null
        };

        return fallbackNode?.Find(packedTypes, configuredFloatType, index + 1);
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

      var nextNode = paramTypes[index] switch
      {
        Type.Integer => _integerNode ??= new CustomFunctionOverloadNode(),
        Type.Long => _longNode ??= new CustomFunctionOverloadNode(),
        Type.Float => _floatNode ??= new CustomFunctionOverloadNode(),
        Type.Double => _doubleNode ??= new CustomFunctionOverloadNode(),
        Type.Decimal => _decimalNode ??= new CustomFunctionOverloadNode(),
        Type.Boolean => _booleanNode ??= new CustomFunctionOverloadNode(),
        Type.String => _stringNode ??= new CustomFunctionOverloadNode(),
        _ => throw new ArgumentOutOfRangeException(nameof(paramTypes), paramTypes[index], "Unsupported parameter type")
      };

      nextNode.Build(function, index + 1);
    }
  }
}