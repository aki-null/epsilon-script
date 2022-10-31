using System.Collections.Generic;

namespace EpsilonScript.Helper
{
  public static class UniqueIdentifierExtension
  {
    private static readonly Dictionary<string, uint> StringIdCache = new Dictionary<string, uint>();
    private static uint _currentId = 1;

    public static uint GetUniqueIdentifier(this string s)
    {
      lock (StringIdCache)
      {
        if (StringIdCache.TryGetValue(s, out var id))
        {
          return id;
        }

        id = _currentId;
        StringIdCache[s] = id;
        ++_currentId;
        return id;
      }
    }

    public static void ResetUniqueIdentifierCache()
    {
      StringIdCache.Clear();
      _currentId = 1;
    }
  }
}