using System.Collections.Generic;

namespace EpsilonScript.Helper
{
  public static class UniqueIdentifierExtension
  {
    private static readonly Dictionary<string, uint> StringIdCache = new Dictionary<string, uint>();
    private static readonly List<string> StringIdReverseLookup = new List<string>();
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
        StringIdReverseLookup.Add(s);
        ++_currentId;
        return id;
      }
    }

    public static string GetStringFromUniqueIdentifier(this uint s)
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