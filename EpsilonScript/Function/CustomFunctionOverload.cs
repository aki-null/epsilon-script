using System;

namespace EpsilonScript.Function
{
  internal class CustomFunctionOverload
  {
    private readonly CustomFunctionOverloadNode _rootNode;

    public VariableId Name { get; }
    public bool IsDeterministic { get; }
    private Compiler.FloatPrecision ConfiguredFloatType { get; }

    public CustomFunctionOverload(CustomFunction function, Compiler.FloatPrecision floatPrecision)
    {
      Name = function.Name;
      IsDeterministic = function.IsDeterministic;
      ConfiguredFloatType = floatPrecision;
      _rootNode = new CustomFunctionOverloadNode();
      _rootNode.Build(function);
    }

    public void Add(CustomFunction function)
    {
      if (IsDeterministic != function.IsDeterministic)
      {
        throw new ArgumentException("All functions with the same name must have the same determinism");
      }

      _rootNode.Build(function);
    }

    internal CustomFunction Find(PackedParameterTypes packedTypes)
    {
      return _rootNode.Find(packedTypes, ConfiguredFloatType);
    }
  }
}