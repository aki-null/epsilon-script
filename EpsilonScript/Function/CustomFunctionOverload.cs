using System;

namespace EpsilonScript.Function
{
  public class CustomFunctionOverload
  {
    private CustomFunctionOverloadNode _rootNode;

    public string Name { get; }
    public bool IsConstant { get; }

    public CustomFunctionOverload(CustomFunction function)
    {
      Name = function.Name;
      IsConstant = function.IsConstant;
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

    public CustomFunction Find(Type[] paramTypes)
    {
      return _rootNode.Find(paramTypes);
    }
  }
}