using System.Collections.Generic;

namespace EpsilonScript.Function
{
  public class CustomFunctionContainer
  {
    private readonly Dictionary<int, CustomFunctionOverload> _functionMap =
      new Dictionary<int, CustomFunctionOverload>();

    internal bool Query(int name, out CustomFunctionOverload function)
    {
      return _functionMap.TryGetValue(name, out function);
    }

    public void Add(CustomFunction func)
    {
      if (_functionMap.TryGetValue(func.Name, out var overload))
      {
        overload.Add(func);
      }
      else
      {
        _functionMap[func.Name] = new CustomFunctionOverload(func);
      }
    }
  }
}