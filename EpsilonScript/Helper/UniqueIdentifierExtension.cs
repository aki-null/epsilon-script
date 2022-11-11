using System.Collections.Generic;

namespace EpsilonScript.Helper
{
  public static class UniqueIdentifierExtension
  {
    private static readonly Dictionary<string, int> StringIdCache = new Dictionary<string, int>();
    private static readonly List<string> StringIdReverseLookup = new List<string>();
    private static int _currentId = 1;

    public static int GetUniqueIdentifier(this string s)
    {
      lock (StringIdCache)
      {
        if (StringIdCache.TryGetValue(s, out var id))
        {
          return id;
        }

        id = _currentId;
        StringIdCache[s] = id;
        StringIdReverseLookup.Add(s);
        ++_currentId;
        return id;
      }
    }

    public static string GetStringFromUniqueIdentifier(this int s)
    {
      lock (StringIdCache)
      {
        var lookupPos = s - 1;
        return StringIdReverseLookup[(int)lookupPos];
      }
    }

    public static void ResetUniqueIdentifierCache()
    {
      StringIdCache.Clear();
      _currentId = 1;
    }
  }
}