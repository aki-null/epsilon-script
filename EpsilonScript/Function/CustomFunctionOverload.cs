using System;

namespace EpsilonScript.Function
{
  internal class CustomFunctionOverload
  {
    private readonly CustomFunctionOverloadNode _rootNode;

    public VariableId Name { get; }
    public bool IsConstant { get; }
    private Compiler.FloatPrecision ConfiguredFloatType { get; }

    public CustomFunctionOverload(CustomFunction function, Compiler.FloatPrecision floatPrecision)
    {
      Name = function.Name;
      IsConstant = function.IsConstant;
      ConfiguredFloatType = floatPrecision;
      _rootNode = new CustomFunctionOverloadNode();
      _rootNode.Build(function);
    }

    public void Add(CustomFunction function)
    {
      if (IsConstant != function.IsConstant)
      {
        throw new ArgumentException("All functions with the same name must have the same constness");
      }

      _rootNode.Build(function);
    }

    internal CustomFunction Find(PackedParameterTypes packedTypes)
    {
      return _rootNode.Find(packedTypes, ConfiguredFloatType);
    }
  }
}