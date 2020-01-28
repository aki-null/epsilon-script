using System;
using EpsilonScript.Parser;

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
    private CustomFunctionOverloadNode _floatNode;
    private CustomFunctionOverloadNode _booleanNode;

    private static CustomFunction FindOverload(Type[] paramTypes, int index, CustomFunctionOverloadNode primary,
      CustomFunctionOverloadNode secondary)
    {
      CustomFunction function = null;
      if (primary != null)
      {
        function = primary.Find(paramTypes, index + 1);
      }

      if (function == null && secondary != null)
      {
        function = secondary.Find(paramTypes, index + 1);
      }

      return function;
    }

    public CustomFunction Find(Type[] paramTypes, int index = 0)
    {
      if (index >= paramTypes.Length)
      {
        return LeafFunction;
      }

      switch (paramTypes[index])
      {
        case Type.Integer:
          return FindOverload(paramTypes, index, _integerNode, _floatNode);
        case Type.Float:
          return FindOverload(paramTypes, index, _floatNode, _integerNode);
        case Type.Boolean:
          return _booleanNode?.Find(paramTypes, index + 1);
        default:
          throw new ArgumentOutOfRangeException(nameof(paramTypes), paramTypes[index], "Unsupported parameter type");
      }
    }

    public void Build(CustomFunction function, int index = 0)
    {
      var paramTypes = function.ParameterTypes;
      if (index >= paramTypes.Length)
      {
        if (LeafFunction != null)
        {
          throw new RuntimeException("The custom function with same name and same parameter types is already defined");
        }

        LeafFunction = function;
        return;
      }

      CustomFunctionOverloadNode nextNode;
      switch (paramTypes[index])
      {
        case Type.Integer:
          if (_integerNode == null)
          {
            _integerNode = new CustomFunctionOverloadNode();
          }

          nextNode = _integerNode;
          break;
        case Type.Float:
          if (_floatNode == null)
          {
            _floatNode = new CustomFunctionOverloadNode();
          }

          nextNode = _floatNode;
          break;
        case Type.Boolean:
          if (_booleanNode == null)
          {
            _booleanNode = new CustomFunctionOverloadNode();
          }

          nextNode = _booleanNode;
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(paramTypes), paramTypes[index], "Unsupported parameter type");
      }

      nextNode.Build(function, index + 1);
    }
  }
}