using System;
using System.Collections.Generic;

namespace EpsilonScript.Function
{
  internal class CustomFunctionOverload
  {
    private readonly CustomFunctionOverloadNode _rootNode;

    private readonly Dictionary<PackedParameterTypes, CustomFunction> _lookupCache =
      new Dictionary<PackedParameterTypes, CustomFunction>(PackedParameterTypesComparer.Instance);

    public VariableId Name { get; }
    public bool IsDeterministic { get; }
    public int Version { get; private set; }

    public CustomFunctionOverload(CustomFunction function, Compiler.IntegerPrecision intPrecision,
      Compiler.FloatPrecision floatPrecision)
    {
      Name = function.Name;
      IsDeterministic = function.IsDeterministic;
      _rootNode = new CustomFunctionOverloadNode(intPrecision, floatPrecision);
      _rootNode.Build(function);
    }

    public void Add(CustomFunction function)
    {
      if (IsDeterministic != function.IsDeterministic)
      {
        throw new ArgumentException(
          $"Function '{Name}': cannot mix deterministic and non-deterministic overloads. " +
          $"Existing overloads are {(IsDeterministic ? "deterministic" : "non-deterministic")}, " +
          $"but trying to add: {function}");
      }

      _rootNode.Build(function);
      _lookupCache.Clear(); // New overloads invalidate cached lookups
      unchecked
      {
        Version++;
      }
    }

    internal CustomFunction Find(PackedParameterTypes packedTypes)
    {
      if (_lookupCache.TryGetValue(packedTypes, out var cached))
      {
        return cached;
      }

      var function = _rootNode.Find(packedTypes);
      if (function != null)
      {
        _lookupCache[packedTypes] = function;
      }

      return function;
    }

    internal System.Collections.Generic.List<CustomFunction> GetAllOverloads()
    {
      var functions = new System.Collections.Generic.List<CustomFunction>();
      _rootNode.CollectAllFunctions(functions);
      return functions;
    }
  }
}