using System.Collections.Generic;

namespace EpsilonScript.Helper
{
  public static class ContainerExtension
  {
    public static bool TryPop<T>(this Stack<T> s, out T obj)
    {
      if (s.Count == 0)
      {
        obj = default;
        return false;
      }

      obj = s.Pop();
      return true;
    }

    public static bool TryPeek<T>(this Stack<T> s, out T obj)
    {
      if (s.Count == 0)
      {
        obj = default;
        return false;
      }

      obj = s.Peek();
      return true;
    }
  }
}