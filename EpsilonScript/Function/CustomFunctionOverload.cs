using System;

namespace EpsilonScript.Function
{
  internal class CustomFunctionOverload
  {
    private readonly CustomFunctionOverloadNode _rootNode;

    public VariableId Name { get; }
    public bool IsDeterministic { get; }

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
    }

    internal CustomFunction Find(PackedParameterTypes packedTypes)
    {
      return _rootNode.Find(packedTypes);
    }

    internal System.Collections.Generic.List<CustomFunction> GetAllOverloads()
    {
      var functions = new System.Collections.Generic.List<CustomFunction>();
      _rootNode.CollectAllFunctions(functions);
      return functions;
    }
  }
}